using Zenject;
using UnityEngine;
using IPA.Utilities;
using SiraUtil.Tools;
using DiBris.Managers;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;

namespace DiBris.Components
{
    internal class DiSpawner : NoteDebrisSpawner
    {
        private SiraLog _siraLog = null!;
        private readonly List<(bool, Config)> _configs = new List<(bool, Config)>();
        private readonly ConditionalWeakTable<NoteDebris, NoteDebrisRigidbodyPhysics> _physicsTable = new ConditionalWeakTable<NoteDebris, NoteDebrisRigidbodyPhysics>();

        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.Accessor SimplePhysics = FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.GetAccessor("_simplePhysics");
        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.Accessor RigidBody = FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.GetAccessor("_rigidbody");
        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, bool>.Accessor RigidFirstUpdate = FieldAccessor<NoteDebrisRigidbodyPhysics, bool>.GetAccessor("_firstUpdate");
        private static readonly FieldAccessor<NoteDebrisSimplePhysics, bool>.Accessor SimpleFirstUpdate = FieldAccessor<NoteDebrisSimplePhysics, bool>.GetAccessor("_firstUpdate");
        private static readonly FieldAccessor<NoteDebrisSimplePhysics, Vector3>.Accessor Gravity = FieldAccessor<NoteDebrisSimplePhysics, Vector3>.GetAccessor("_gravity");
        private static readonly FieldAccessor<NoteDebris, NoteDebrisPhysics>.Accessor RigidPhysics = FieldAccessor<NoteDebris, NoteDebrisPhysics>.GetAccessor("_physics");

        [Inject]
        protected async Task Construct(Config config, SiraLog siraLog, ProfileManager profiles, IDifficultyBeatmap beatmap)
        {
            _siraLog = siraLog;
            List<Config> unBrokenConfigs = new List<Config> { config };
            unBrokenConfigs.AddRange(await profiles.GetMirrorConfigs());
            foreach (var conf in unBrokenConfigs)
            {
                var parameters = conf.Parameters;
                bool shouldBreak = false;
                bool failsLength = false;
                bool failsNJS = false;
                bool failsNPS = false;
                if (parameters.DoLength)
                {
                    siraLog.Info(beatmap.level.songDuration);
                    if (beatmap.level.songDuration >= parameters.Length)
                        failsLength = true;
                }
                if (!shouldBreak && parameters.DoNJS)
                {
                    if (beatmap.noteJumpMovementSpeed >= parameters.NJS)
                        failsNJS = true;
                }
                if (parameters.DoNPS)
                {
                    var data = beatmap.beatmapData;
                    var levelData = beatmap.level.beatmapLevelData;
                    if (data.cuttableNotesType / levelData.audioClip.length >= parameters.NPS)
                        failsNPS = true;
                }
                if (parameters.Mode == Models.DisableMode.All)
                {
                    shouldBreak = (!parameters.DoLength || failsLength) && (!parameters.DoNJS || failsNJS) && (!parameters.DoNPS || failsNPS);
                }
                else
                {
                    shouldBreak = failsLength || failsNJS || failsNPS;
                }
                _configs.Add((!shouldBreak, conf));
            }
        }

        public override void SpawnDebris(Vector3 cutPoint, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, Vector3 notePos, Quaternion noteRotation, Vector3 noteScale, ColorType colorType, float timeToNextColorNote, Vector3 moveVec)
        {
            foreach (var config in _configs)
            {
                Config conf = config.Item2;
                if (conf.RemoveDebris || !config.Item1)
                    return;

                Vector3 newPos = notePos;
                newPos *= conf.AbsolutePositionScale;
                newPos += conf.AbsolutePositionOffset;
                bool shouldInteract = conf.VelocityMultiplier != 0;

                Quaternion noteRot = noteRotation;
                if (conf.FixateRotationToZero)
                    noteRot = Quaternion.identity;
                
                if (conf.SnapToGrid)
                {
                    var snapScale = conf.GridScale / 4f;
                    float snapX = System.Convert.ToSingle(System.Math.Round(newPos.x, System.MidpointRounding.ToEven) * snapScale);
                    float snapY = System.Convert.ToSingle(System.Math.Round(newPos.y, System.MidpointRounding.ToEven) * snapScale);
                    float snapZ = System.Convert.ToSingle(System.Math.Round(newPos.z, System.MidpointRounding.ToEven) * snapScale);
                    newPos = new Vector3(snapX, snapY, snapZ);
                }

                if (conf.FixateZPos)
                {
                    newPos = new Vector3(newPos.x, newPos.y, conf.AbsolutePositionOffsetZ);
                }

                var debrisA = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque);
                debrisA.transform.localScale *= conf.Scale;
                if (conf.FixedLifetime) liquid = conf.FixedLifetimeLength;
                debrisA.Init(colorType, newPos, noteRot, noteScale, transform.position, transform.rotation, cutPoint, -cutNormal, (-forceEn * _fromCenterSpeed + next) * conf.VelocityMultiplier, -torque * conf.RotationMultiplier, liquid * conf.LifetimeMultiplier);
                StartCoroutine(MultiplyGravity(debrisA, conf.GravityMultiplier, shouldInteract));

                var debrisB = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid2, out Vector3 next2, out Vector3 forceEn2, out Vector3 torque2);
                debrisB.transform.localScale *= conf.Scale;
                if (conf.FixedLifetime) liquid2 = conf.FixedLifetimeLength;
                debrisB.Init(colorType, newPos, noteRot, noteScale, transform.position, transform.rotation, cutPoint, cutNormal, (forceEn2 * _fromCenterSpeed + next2) * conf.VelocityMultiplier, torque2 * conf.RotationMultiplier, liquid2 * conf.LifetimeMultiplier);
                StartCoroutine(MultiplyGravity(debrisB, conf.GravityMultiplier, shouldInteract));
            }
        }

        private NoteDebris DebrisDecorator(float cutY, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, float timeToNextColorNote, Vector3 moveVec, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque)
        {
            var debris = _noteDebrisPool.Spawn();
            debris.didFinishEvent.Add(this);
            debris.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            float magnitude = moveVec.magnitude;
            liquid = Mathf.Clamp(timeToNextColorNote + 0.05f, 0.2f, 2f);
            var projection = Vector3.ProjectOnPlane(saberDir, moveVec / magnitude);
            next = projection * (saberSpeed * _cutDirMultiplier) + moveVec * _moveSpeedMultiplier;
            forceEn = transform.rotation * (cutNormal + Random.onUnitSphere * 0.1f);
            torque = transform.rotation * Vector3.Cross(cutNormal, projection) * _rotation / Mathf.Max(1f, timeToNextColorNote * 2f);
            next.y = cutY >= 1.3 ? Mathf.Max(next.y, 0f) : Mathf.Min(next.y, 0);

            if (!_physicsTable.TryGetValue(debris, out NoteDebrisRigidbodyPhysics rigidPhysics))
            {
                rigidPhysics = (RigidPhysics(ref debris) as NoteDebrisRigidbodyPhysics)!;
                _physicsTable.Add(debris, rigidPhysics);
            }

            return debris;
        }

        private IEnumerator MultiplyGravity(NoteDebris noteDebris, float multiplier, bool shouldInteract)
        {
            yield return new WaitForEndOfFrame();
            if (_physicsTable.TryGetValue(noteDebris, out NoteDebrisRigidbodyPhysics rigidPhysics))
            {
                MultiplyGravityInternal(rigidPhysics, multiplier, shouldInteract);
            }
            else
            {
                MultiplyGravityInternal((RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!, multiplier, shouldInteract);
            }
        }

        private void MultiplyGravityInternal(NoteDebrisRigidbodyPhysics rigidPhysics, float multiplier, bool shouldInteract)
        {
            var simplePhysics = SimplePhysics(ref rigidPhysics);

            SimpleFirstUpdate(ref simplePhysics) = true;
            RigidFirstUpdate(ref rigidPhysics) = true;

            Gravity(ref simplePhysics) *= multiplier;
            if (multiplier == 0)
            {
                RigidBody(ref rigidPhysics).useGravity = false;
            }
            else if (0 > multiplier)
            {
                rigidPhysics.enabled = false;
                simplePhysics.enabled = true;
            }
            RigidBody(ref rigidPhysics).isKinematic = !shouldInteract;
        }

        public new void HandleNoteDebrisDidFinish(NoteDebris noteDebris)
        {
            noteDebris.transform.localScale = Vector3.one;
            noteDebris.didFinishEvent.Remove(this);
            if (_physicsTable.TryGetValue(noteDebris, out NoteDebrisRigidbodyPhysics rigidPhysics))
            {
                var simplePhysics = SimplePhysics(ref rigidPhysics);
                RigidBody(ref rigidPhysics).isKinematic = false;
                RigidBody(ref rigidPhysics).useGravity = true;
                Gravity(ref simplePhysics) = Physics.gravity;
            }
            _noteDebrisPool.Despawn(noteDebris);
        }
    }
}