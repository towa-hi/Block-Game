// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;

// // all entities have this as the base component. holds important gameplay data and entity state
// // but doesn't hold graphics data or graphics state
// [SelectionBase]
// public class EntityBase : SerializedMonoBehaviour {
//     private HashSet<IComponent> iComponentSet;
//     public EntityView entityView;
//     public EntityData entityData;
//     public bool isInTempPos;
//     public bool isDying;
//     DeathType deathType;
//     public Animator animator;
//     float t;
//     public float originalHeight;

//     // we want to initialize entityBase, all the iComponents and entityView with entityData
//     public void Init(EntityData aEntityData) {
//         this.entityData = aEntityData;
//         this.entityView = this.transform.GetChild(0).GetComponent<EntityView>();
//         this.name = this.entityData.name;
//         this.iComponentSet = new HashSet<IComponent>();
//         this.entityView.Init(this.entityData);
//         ResetViewPosition();
//         foreach (IComponent iComponent in GetComponents(typeof(IComponent))) {
//             iComponentSet.Add(iComponent);
//             iComponent.Init();
//         }
//         this.entityData.componentsAreInitialized = true;
//         this.isDying = false;
//     }

//     public IComponent GetCachedIComponent<T>() {
//         foreach (IComponent iComponent in this.iComponentSet) {
//             if (iComponent.GetType() == typeof(T)) {
//                 return iComponent;
//             }
//         }
//         return null;
//     }

//     public void DoFrame() {
//         if (this.isDying) {
//             if (this.t < 1) {
//                 t += Time.deltaTime / Constants.DEATHSTATETIME;
//                 this.entityView.myRenderer.material.color = Color.Lerp(this.entityData.defaultColor, Color.black, t);
//             } else {
//                 print("dying for real now");
//                 GM.playManager.FinishEntityDeath(this.entityData);
//             }
//         } else {
//             foreach (IComponent component in iComponentSet) {
//             component.DoFrame();
//         }
//         }
        
//     }

//     public void Die(DeathType aDeathType) {
//         this.isDying = true;
//         this.deathType = aDeathType;
//         switch (this.deathType) {
//             case DeathType.BUMP:   
//                 break;
//             case DeathType.FIRE:
//                 break;
//             case DeathType.BISECTED:
//                 break;
//             case DeathType.SQUISHED:
//                 print("squished");
//                 animator.SetTrigger("Squish");
//                 break;
//         }
//     }

//     void DeathUpdate() {
//         switch (this.deathType) {
//             case DeathType.BUMP:
//                 if (this.t < 1) {
//                     t += Time.deltaTime / Constants.DEATHSTATETIME;
//                     this.entityView.myRenderer.material.color = Color.Lerp(this.entityData.defaultColor, Color.black, t);
//                 }
//                 break;
//             case DeathType.FIRE:
//                 if (this.t < 1) {
//                     t += Time.deltaTime / Constants.DEATHSTATETIME;
//                 }
//                 break;
//             case DeathType.BISECTED:
//                 if (this.t < 1) {
//                     t += Time.deltaTime / Constants.DEATHSTATETIME;
//                 }
//                 break;
//             case DeathType.SQUISHED:
//                 if (this.t < 1) {
//                     t += Time.deltaTime / Constants.DEATHSTATETIME;

//                 }
//                 break;
//         }
//     }

//     public void SetViewPosition(Vector2Int aPos) {
//         this.transform.position = Util.V2IOffsetV3(aPos, this.entityData.size);
//         this.isInTempPos = true;
//     }

//     public void ResetViewPosition() {
//         this.transform.position = Util.V2IOffsetV3(this.entityData.pos, this.entityData.size);
//         this.isInTempPos = false;
//         if (this.entityData.facing == Vector2Int.right) {
//             this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
//         } else if (this.entityData.facing == Vector2Int.left) {
//             this.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
//         }
//     }
//     // TODO: have a reset that lerps quickly back to pos
// }

