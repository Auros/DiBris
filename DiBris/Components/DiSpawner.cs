using Zenject;
using UnityEngine;
using IPA.Utilities;
using Random = UnityEngine.Random;

namespace DiBris.Components
{
    internal class DiSpawner : NoteDebrisSpawner
    {
        private Config _config = null!;

        private static FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.Accessor SimplePhysics = FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.GetAccessor("_simplePhysics");
        private static FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.Accessor RigidBody = FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.GetAccessor("_rigidbody");
        private static FieldAccessor<NoteDebrisSimplePhysics, Vector3>.Accessor Gravity = FieldAccessor<NoteDebrisSimplePhysics, Vector3>.GetAccessor("_gravity");
        private static FieldAccessor<NoteDebris, NoteDebrisPhysics>.Accessor RigidPhysics = FieldAccessor<NoteDebris, NoteDebrisPhysics>.GetAccessor("_physics");

        [Inject]
        protected void Construct(Config config)
        {
            _config = config;
        }

        public override void SpawnDebris(Vector3 cutPoint, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, Vector3 notePos, Quaternion noteRotation, ColorType colorType, float timeToNextColorNote, Vector3 moveVec)
        {
            if (_config.RemoveDebris)
            {
                return;
            }

            var debrisA = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque);
            debrisA.transform.localScale *= _config.Scale;
            debrisA.Init(colorType, notePos, noteRotation, transform.position, transform.rotation, cutPoint, -cutNormal, (-forceEn * _fromCenterSpeed + next) * _config.VelocityMultiplier, -torque, liquid * _config.LifetimeMultiplier);

            var debrisB = DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, out float liquid2, out Vector3 next2, out Vector3 forceEn2, out Vector3 torque2);
            debrisB.transform.localScale *= _config.Scale;
            debrisB.Init(colorType, notePos, noteRotation, transform.position, transform.rotation, cutPoint, cutNormal, (forceEn2 * _fromCenterSpeed + next2) * _config.VelocityMultiplier, torque2, liquid2 * _config.LifetimeMultiplier);

            MultiplyGravity(debrisA, _config.GravityMultiplier);
            MultiplyGravity(debrisB, _config.GravityMultiplier);
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
            return debris;
        }

        private void MultiplyGravity(NoteDebris noteDebris, float multiplier)
        {
            var rigidPhysics = (RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!;
            var simplePhysics = SimplePhysics(ref rigidPhysics);

            Gravity(ref simplePhysics) *= multiplier;
            RigidBody(ref rigidPhysics).useGravity = multiplier != 0;
        }

        public override void HandleNoteDebrisDidFinish(NoteDebris noteDebris)
        {
            noteDebris.transform.localScale = Vector3.one;
            var rigidPhysics = (RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!;
            var simplePhysics = SimplePhysics(ref rigidPhysics);
            Gravity(ref simplePhysics) = Physics.gravity;
        }
    }
}