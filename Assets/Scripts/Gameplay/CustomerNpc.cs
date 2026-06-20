using System.Collections;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SupermarketSim.Gameplay
{
    public class CustomerNpc : MonoBehaviour
    {
        private const float MoveSpeed = 3.2f;
        private const float ReachDistance = 0.25f;
        private const float ItemReachDistance = 1.75f;
        private const float CashierReachDistance = 1.75f;

        private Transform handPoint;
        private PickupableItem carriedItem;
        private Vector3 exitPosition;
        private CharacterController controller;
        private NavMeshAgent agent;
        private GameObject visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform headBone;
        private Vector3 visualBaseLocalPosition;
        private bool isMoving;

        public void Initialize(Vector3 exitPoint)
        {
            exitPosition = exitPoint;
            BuildVisual();
            StartCoroutine(StartWhenNavigationReady());
        }

        private IEnumerator StartWhenNavigationReady()
        {
            float endTime = Time.time + 5f;
            while (agent != null && !agent.isOnNavMesh && Time.time < endTime)
            {
                if (NavMesh.SamplePosition(transform.position, out var hit, 8f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    break;
                }

                yield return null;
            }

            if (agent != null && !agent.isOnNavMesh)
                agent.enabled = false;

            StartCoroutine(CustomerRoutine());
        }

        private void BuildVisual()
        {
            gameObject.name = "CustomerNpc";

            if (!TryBuildKenneyEmployeeVisual())
                BuildPrimitiveCustomerVisual();

            if (visualRoot != null)
                visualBaseLocalPosition = visualRoot.transform.localPosition;

            handPoint = new GameObject("HandPoint").transform;
            handPoint.SetParent(transform, false);
            handPoint.localPosition = new Vector3(0.45f, 1.1f, 0.45f);

            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.enabled = false;

            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = MoveSpeed;
            agent.angularSpeed = 720f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.4f;
            agent.radius = 0.38f;
            agent.height = 2f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(30, 70);
            PlayerOnlyStoreBoundary.ConfigureNpcCollision(gameObject);
        }

        private bool TryBuildKenneyEmployeeVisual()
        {
#if UNITY_EDITOR
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/character-employee.obj");
            if (model == null)
                model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_MiniMarket/Models/FBX format/character-employee.fbx");

            if (model == null) return false;

            visualRoot = (GameObject)Instantiate(model, transform);
            visualRoot.name = "KenneyEmployeeVisual";
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one * 1.65f;

            foreach (var collider in visualRoot.GetComponentsInChildren<Collider>(true))
                Destroy(collider);

            foreach (var child in visualRoot.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            FixVisualMaterials(visualRoot);
            return true;
#else
            return false;
#endif
        }

        private void BuildPrimitiveCustomerVisual()
        {
            visualRoot = new GameObject("PrimitiveCustomerVisual");
            visualRoot.transform.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            body.layer = LayerMask.NameToLayer("Ignore Raycast");
            Destroy(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;
            head.layer = LayerMask.NameToLayer("Ignore Raycast");
            Destroy(head.GetComponent<Collider>());

            var color = Random.ColorHSV(0f, 1f, 0.45f, 0.75f, 0.65f, 1f);
            SetRendererColor(body.GetComponent<Renderer>(), color);
            SetRendererColor(head.GetComponent<Renderer>(), new Color(1f, 0.78f, 0.58f));
            headBone = head.transform;

            leftArm = CreatePrimitiveLimb("LeftArm", new Vector3(-0.48f, 1.15f, 0f), new Vector3(0.16f, 0.75f, 0.16f), color * 0.9f);
            rightArm = CreatePrimitiveLimb("RightArm", new Vector3(0.48f, 1.15f, 0f), new Vector3(0.16f, 0.75f, 0.16f), color * 0.9f);
            leftLeg = CreatePrimitiveLimb("LeftLeg", new Vector3(-0.22f, 0.35f, 0f), new Vector3(0.18f, 0.7f, 0.18f), new Color(0.18f, 0.22f, 0.38f));
            rightLeg = CreatePrimitiveLimb("RightLeg", new Vector3(0.22f, 0.35f, 0f), new Vector3(0.18f, 0.7f, 0.18f), new Color(0.18f, 0.22f, 0.38f));
        }

        private Transform CreatePrimitiveLimb(string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var limb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            limb.name = name;
            limb.transform.SetParent(visualRoot.transform, false);
            limb.transform.localPosition = localPosition;
            limb.transform.localScale = localScale;
            limb.layer = LayerMask.NameToLayer("Ignore Raycast");
            Destroy(limb.GetComponent<Collider>());
            SetRendererColor(limb.GetComponent<Renderer>(), color);
            return limb.transform;
        }

        private static void FixVisualMaterials(GameObject root)
        {
            var shader = Shader.Find("HDRP/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            if (shader == null) return;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var oldMat = materials[i];
                    var mat = new Material(shader);
                    var color = Color.Lerp(Color.white, Random.ColorHSV(0f, 1f, 0.45f, 0.75f, 0.7f, 1f), 0.35f);
                    Texture tex = null;
                    if (oldMat != null && oldMat.HasProperty("_Color")) color = oldMat.GetColor("_Color");
                    if (oldMat != null && oldMat.HasProperty("_BaseColor")) color = oldMat.GetColor("_BaseColor");
                    if (oldMat != null && oldMat.HasProperty("_MainTex")) tex = oldMat.GetTexture("_MainTex");
                    if (oldMat != null && tex == null && oldMat.HasProperty("_BaseMap")) tex = oldMat.GetTexture("_BaseMap");
                    if (oldMat != null && tex == null && oldMat.HasProperty("_BaseColorMap")) tex = oldMat.GetTexture("_BaseColorMap");
                    color = Color.Lerp(color, Random.ColorHSV(0f, 1f, 0.45f, 0.75f, 0.7f, 1f), 0.25f);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                    if (tex != null && mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", tex);
                    if (tex != null && mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    if (tex != null && mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                    materials[i] = mat;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            var shader = Shader.Find("HDRP/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            if (shader == null) return;
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            renderer.material = mat;
        }

        private IEnumerator CustomerRoutine()
        {
            var entrance = new Vector3(0f, 0f, -18f);
            var browsePoint = new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(8f, 18f));

            yield return MoveTo(entrance);
            yield return MoveTo(browsePoint);

            var item = FindAvailableProduct();
            if (item == null)
            {
                yield return LookAroundForSeconds(10f);
                yield return LeaveStore();
                yield break;
            }

            var itemApproach = GetItemApproachPosition(item);
            yield return MoveTo(itemApproach, 0.6f, 10f);
            if (item == null)
            {
                yield return LeaveStore();
                yield break;
            }

            if (Vector3.Distance(Flat(transform.position), Flat(item.transform.position)) > 3.25f)
            {
                yield return LookAroundForSeconds(4f);
                yield return LeaveStore();
                yield break;
            }

            TakeItem(item);

            var cashier = FindCashierPosition(out var cashierLookTarget);
            yield return MoveToCashierQueue(cashier, cashierLookTarget);
            yield return CheckoutRoutine(cashierLookTarget);

            yield return LeaveStore();
        }

        private IEnumerator CheckoutRoutine(Vector3 cashierPoint)
        {
            var cashier = FindNearestCashierStation();
            if (cashier == null || carriedItem == null)
            {
                yield return LookAroundForSeconds(2f);
                yield break;
            }

            while (!cashier.TryPlaceCustomerItem(this, carriedItem))
            {
                FaceTowards(cashierPoint);
                AnimateIdleLook();
                yield return null;
            }

            carriedItem = null;
            while (!cashier.WasCustomerItemScanned(this))
            {
                FaceTowards(cashierPoint);
                AnimateIdleLook();
                yield return null;
            }

            cashier.ClearCustomer(this);
        }

        private CashierStationInteractable FindNearestCashierStation()
        {
            CashierStationInteractable best = null;
            float bestDistance = float.MaxValue;

            foreach (var cashier in Object.FindObjectsByType<CashierStationInteractable>(FindObjectsInactive.Exclude))
            {
                if (cashier == null) continue;
                float distance = Vector3.Distance(Flat(transform.position), Flat(cashier.transform.position));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = cashier;
                }
            }

            return best;
        }

        private void FaceTowards(Vector3 point)
        {
            var dir = Flat(point) - Flat(transform.position);
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        private IEnumerator MoveToCashierQueue(Vector3 cashierQueuePoint, Vector3 cashierLookTarget)
        {
            var preferredDirection = Flat(cashierQueuePoint) - Flat(cashierLookTarget);
            if (preferredDirection.sqrMagnitude < 0.01f)
                preferredDirection = Vector3.back;

            var queuePoint = FindReachableInteractionPoint(cashierLookTarget, 4.5f, preferredDirection.normalized);
            yield return MoveTo(queuePoint, 1.25f, 12f);
            FaceTowards(cashierLookTarget);
        }

        private PickupableItem FindAvailableProduct()
        {
            PickupableItem best = null;
            float bestDistance = float.MaxValue;

            foreach (var item in Object.FindObjectsByType<PickupableItem>(FindObjectsInactive.Exclude))
            {
                if (item == null || item.currentPlacementPoint == null) continue;
                if (item.transform.IsChildOf(transform)) continue;

                float dist = Vector3.Distance(transform.position, item.transform.position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = item;
                }
            }

            if (best != null)
                return best;

            foreach (var item in Object.FindObjectsByType<PickupableItem>(FindObjectsInactive.Exclude))
            {
                if (item == null || item.transform.IsChildOf(transform)) continue;
                if (item.GetComponent<Rigidbody>() == null) continue;

                float dist = Vector3.Distance(transform.position, item.transform.position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = item;
                }
            }

            return best;
        }

        private Vector3 GetItemApproachPosition(PickupableItem item)
        {
            if (item == null)
                return transform.position;

            var itemPos = Flat(item.transform.position);
            var storeCenter = Vector3.zero;
            Vector3 awayFromShelf = itemPos - storeCenter;

            // Most generated shelves stand along the back wall (z ~= 20), so customers should approach from the aisle.
            if (itemPos.z > 8f)
                awayFromShelf = Vector3.forward;
            else if (itemPos.z < -8f)
                awayFromShelf = Vector3.back;

            if (awayFromShelf.sqrMagnitude < 0.01f)
                awayFromShelf = Vector3.back;

            var approach = FindReachableInteractionPoint(itemPos, 2.2f, -awayFromShelf.normalized);
            approach.y = 0f;
            return approach;
        }

        private Vector3 FindReachableInteractionPoint(Vector3 focusPoint, float radius, Vector3 preferredDirection)
        {
            focusPoint = Flat(focusPoint);
            preferredDirection = Flat(preferredDirection);
            if (preferredDirection.sqrMagnitude < 0.01f)
                preferredDirection = Vector3.back;

            Vector3 bestPoint = CustomerNavMeshRuntime.NearestNavMeshPoint(focusPoint - preferredDirection.normalized * radius);
            float bestScore = float.MaxValue;
            var start = CustomerNavMeshRuntime.NearestNavMeshPoint(transform.position);

            for (int i = 0; i < 16; i++)
            {
                float angle = i * 22.5f;
                var direction = Quaternion.Euler(0f, angle, 0f) * preferredDirection.normalized;
                var candidate = focusPoint + direction * radius;

                if (!NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
                    continue;

                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(start, hit.position, NavMesh.AllAreas, path))
                    continue;
                if (path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float preference = Vector3.Angle(preferredDirection.normalized, direction) * 0.03f;
                float distance = GetPathLength(path);
                float score = distance + preference;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPoint = hit.position;
                }
            }

            return bestPoint;
        }

        private static float GetPathLength(NavMeshPath path)
        {
            if (path == null || path.corners == null || path.corners.Length < 2)
                return 0f;

            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return length;
        }

        private Vector3 FindCashierPosition(out Vector3 lookTarget)
        {
            var cashier = Object.FindAnyObjectByType<CashierStationInteractable>();
            if (cashier == null)
            {
                lookTarget = new Vector3(0f, 0f, -10f);
                return new Vector3(0f, 0f, -14f);
            }

            lookTarget = cashier.transform.position;
            var target = cashier.transform.position + Vector3.back * 6f;
            target.y = 0f;
            return target;
        }

        private void TakeItem(PickupableItem item)
        {
            carriedItem = item;

            if (item.currentPlacementPoint != null)
            {
                item.currentPlacementPoint.currentItem = null;
                item.currentPlacementPoint = null;
            }

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            foreach (var col in item.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            item.transform.SetParent(handPoint, true);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale *= 0.75f;
        }

        private IEnumerator LookAroundForSeconds(float seconds)
        {
            float endTime = Time.time + seconds;
            while (Time.time < endTime)
            {
                transform.Rotate(Vector3.up, 80f * Time.deltaTime);
                AnimateIdleLook();
                yield return null;
            }
        }

        private IEnumerator LeaveStore()
        {
            yield return MoveTo(new Vector3(0f, 0f, -24f), ReachDistance, 20f);
            yield return MoveTo(exitPosition, ReachDistance, 25f);
            Destroy(gameObject);
        }

        private IEnumerator MoveTo(Vector3 target, float stopDistance = ReachDistance, float timeoutSeconds = 25f, bool ignoreObstacles = false)
        {
            target.y = 0f;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                yield return MoveToWithAgent(target, stopDistance, timeoutSeconds);
                yield break;
            }

            if (controller != null && !controller.enabled)
            {
                if (agent != null)
                    agent.enabled = false;
                controller.enabled = true;
            }

            float endTime = Time.time + timeoutSeconds;
            float stuckTimer = 0f;
            Vector3 lastPosition = transform.position;
            isMoving = true;

            while (Vector3.Distance(Flat(transform.position), target) > stopDistance && Time.time < endTime)
            {
                var current = Flat(transform.position);
                var direction = target - current;
                if (direction.sqrMagnitude > 0.01f)
                {
                    var moveDirection = ignoreObstacles ? direction.normalized : GetObstacleAwareDirection(direction.normalized);
                    if (moveDirection.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 8f * Time.deltaTime);
                        if (controller != null)
                        {
                            controller.Move((moveDirection * MoveSpeed + Vector3.down * 3f) * Time.deltaTime);
                        }
                        else
                        {
                            var next = Vector3.MoveTowards(current, current + moveDirection, MoveSpeed * Time.deltaTime);
                            transform.position = new Vector3(next.x, 0f, next.z);
                        }
                    }
                }

                AnimateMovement();

                if (Vector3.Distance(transform.position, lastPosition) < 0.03f)
                    stuckTimer += Time.deltaTime;
                else
                    stuckTimer = 0f;

                lastPosition = transform.position;

                if (stuckTimer > 2.5f && stopDistance < 2.5f)
                    stopDistance = 2.5f;

                yield return null;
            }

            isMoving = false;
        }

        private IEnumerator MoveToWithAgent(Vector3 target, float stopDistance, float timeoutSeconds)
        {
            target = CustomerNavMeshRuntime.NearestNavMeshPoint(target);
            agent.isStopped = false;
            agent.stoppingDistance = Mathf.Max(0.2f, stopDistance);
            agent.SetDestination(target);
            isMoving = true;

            float endTime = Time.time + timeoutSeconds;
            while (Time.time < endTime)
            {
                AnimateMovement();

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
                    break;

                if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    break;

                yield return null;
            }

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            isMoving = false;
        }

        private Vector3 GetObstacleAwareDirection(Vector3 desired)
        {
            var origin = transform.position + Vector3.up * 1f;
            const float probeDistance = 1.1f;

            if (!HitsObstacle(origin, desired, probeDistance))
                return desired;

            var right = Quaternion.Euler(0f, 55f, 0f) * desired;
            if (!HitsObstacle(origin, right, probeDistance))
                return right.normalized;

            var left = Quaternion.Euler(0f, -55f, 0f) * desired;
            if (!HitsObstacle(origin, left, probeDistance))
                return left.normalized;

            return Vector3.zero;
        }

        private void Update()
        {
            if (!isMoving)
                AnimateIdleLook();
        }

        private void AnimateMovement()
        {
            if (visualRoot == null) return;

            float t = Time.time * 8f;
            visualRoot.transform.localPosition = visualBaseLocalPosition + Vector3.up * (Mathf.Abs(Mathf.Sin(t)) * 0.045f);

            float swing = Mathf.Sin(t) * 28f;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(swing, 0f, 0f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
        }

        private void AnimateIdleLook()
        {
            if (visualRoot == null) return;

            visualRoot.transform.localPosition = visualBaseLocalPosition + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.015f);
            if (headBone != null)
                headBone.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 1.7f) * 22f, 0f);
        }

        private bool HitsObstacle(Vector3 origin, Vector3 direction, float distance)
        {
            foreach (var hit in Physics.SphereCastAll(origin, 0.32f, direction, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.collider.GetComponentInParent<CustomerNpc>() != null) continue;
                return true;
            }

            return false;
        }

        private static Vector3 Flat(Vector3 value)
        {
            return new Vector3(value.x, 0f, value.z);
        }
    }
}
