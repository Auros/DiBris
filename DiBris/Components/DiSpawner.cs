using Zenject;
using UnityEngine;
using IPA.Utilities;
using SiraUtil.Tools;
using DiBris.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace DiBris.Components
{
    internal class DiSpawner : NoteDebrisSpawner
    {
        private SiraLog _siraLog = null!;
        private readonly List<(bool, Config)> _configs = new List<(bool, Config)>();

        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.Accessor SimplePhysics = FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.GetAccessor("_simplePhysics");
        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.Accessor RigidBody = FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.GetAccessor("_rigidbody");
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

        public override void SpawnDebris(Vector3 cutPoint, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, Vector3 notePos, Quaternion noteRotation, ColorType colorType, float timeToNextColorNote, Vector3 moveVec)
        {
            foreach (var config in _configs)
            {
                Config _config = config.Item2;
                if (_config.RemoveDebris || !config.Item1)
                {
                    return;
                }

                _siraLog.Info("Spawning Debris from: " + _config.Name);
                Vector3 newPos = notePos;
                newPos *= _config.AbsolutePositionScale;
                newPos += _config.AbsolutePositionOffset;

                bool shouldInteract = _config.VelocityMultiplier != 0;

                var debrisA = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque);
                debrisA.transform.localScale *= _config.Scale;
                MultiplyGravity(debrisA, _config.GravityMultiplier, shouldInteract);
                debrisA.Init(colorType, newPos, noteRotation, transform.position, transform.rotation, cutPoint, -cutNormal, (-forceEn * _fromCenterSpeed + next) * _config.VelocityMultiplier, -torque * _config.RotationMultiplier, liquid * _config.LifetimeMultiplier);

                var debrisB = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid2, out Vector3 next2, out Vector3 forceEn2, out Vector3 torque2);
                debrisB.transform.localScale *= _config.Scale;
                MultiplyGravity(debrisB, _config.GravityMultiplier, shouldInteract);
                debrisB.Init(colorType, newPos, noteRotation, transform.position, transform.rotation, cutPoint, cutNormal, (forceEn2 * _fromCenterSpeed + next2) * _config.VelocityMultiplier, torque2 * _config.RotationMultiplier, liquid2 * _config.LifetimeMultiplier);
            }
        }

        private NoteDebris DebrisDecorator(float cutY, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, float timeToNextColorNote, Vector3 moveVec, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque)
        {
            var debris = _noteDebrisPool.Spawn();
            debris.didFinishEvent += HandleNoteDebrisDidFinish;
            debris.didFinishEvent += base.HandleNoteDebrisDidFinish;
            debris.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            float magnitude = moveVec.magnitude;
            liquid = Mathf.Clamp(timeToNextColorNote + 0.05f, 0.2f, 2f);
            var projection = Vector3.ProjectOnPlane(saberDir, moveVec / magnitude);
            next = projection * (saberSpeed * _cutDirMultiplier) + moveVec * _moveSpeedMultiplier;
            forceEn = transform.rotation * (cutNormal + Random.onUnitSphere * 0.1f);
            torque = transform.rotation * Vector3.Cross(cutNormal, projection) * _rotation / Mathf.Max(1f, timeToNextColorNote * 2f);
            next.y = cutY >= 1.3 ? Mathf.Max(next.y, 0f) : Mathf.Min(next.y, 0);

            var rigidPhysics = (RigidPhysics(ref debris) as NoteDebrisRigidbodyPhysics)!;
            var simplePhysics = SimplePhysics(ref rigidPhysics);

            return debris;
        }

        private void MultiplyGravity(NoteDebris noteDebris, float multiplier, bool shouldInteract)
        {
            var rigidPhysics = (RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!;
            var simplePhysics = SimplePhysics(ref rigidPhysics);

            Gravity(ref simplePhysics) *= multiplier;
            RigidBody(ref rigidPhysics).useGravity = multiplier != 0;
            RigidBody(ref rigidPhysics).isKinematic = !shouldInteract;
        }

        public override void HandleNoteDebrisDidFinish(NoteDebris noteDebris)
        {
            noteDebris.transform.localScale = Vector3.one;
            var rigidPhysics = (RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!;
            var simplePhysics = SimplePhysics(ref rigidPhysics);
            RigidBody(ref rigidPhysics).isKinematic = false;
            RigidBody(ref rigidPhysics).useGravity = true;
            Gravity(ref simplePhysics) = Physics.gravity;
        }
    }
}