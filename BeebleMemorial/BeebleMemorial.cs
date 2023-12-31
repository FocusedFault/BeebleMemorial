using BepInEx;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace BeebleMemorial
{
  [BepInPlugin("com.Nuxlar.BeebleMemorial", "BeebleMemorial", "1.0.0")]

  public class BeebleMemorial : BaseUnityPlugin
  {
    // 
    private GameObject beebleStatue = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/mdlBeetle.fbx").WaitForCompletion(), "BeebleMemorialStatue");
    private Material beebleStatueMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/MonstersOnShrineUse/matMonstersOnShrineUse.mat").WaitForCompletion();

    public void Awake()
    {
      // Statue
      beebleStatue.name = "BeebleMemorialStatue";
      beebleStatue.AddComponent<NetworkIdentity>();
      beebleStatue.transform.localScale = new Vector3(3f, 3f, 3f);
      beebleStatue.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMaterial = beebleStatueMat;
      beebleStatue.transform.GetChild(1).gameObject.AddComponent<BoxCollider>();

      // Interactable
      BeebleMemorialManager mgr = beebleStatue.AddComponent<BeebleMemorialManager>();
      PurchaseInteraction interaction = beebleStatue.AddComponent<PurchaseInteraction>();
      interaction.contextToken = "Beeble Memorial (E to pay respects)";
      interaction.NetworkdisplayNameToken = "Beeble Memorial (E to pay respects)";
      mgr.purchaseInteraction = interaction;
      beebleStatue.GetComponent<Highlight>().targetRenderer = beebleStatue.GetComponentInChildren<SkinnedMeshRenderer>();
      GameObject something = new GameObject();
      GameObject trigger = Instantiate(something, beebleStatue.transform);
      trigger.AddComponent<BoxCollider>().isTrigger = true;
      trigger.AddComponent<EntityLocator>().entity = beebleStatue;

      InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
      interactableSpawnCard.name = "iscBeebleMemorialStatue";
      interactableSpawnCard.prefab = beebleStatue;
      interactableSpawnCard.sendOverNetwork = true;
      interactableSpawnCard.hullSize = HullClassification.Golem;
      interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
      interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
      interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
      interactableSpawnCard.directorCreditCost = 0;
      interactableSpawnCard.occupyPosition = true;
      interactableSpawnCard.orientToFloor = false;
      interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

      DirectorCard directorCard = new DirectorCard
      {
        selectionWeight = 100,
        spawnCard = interactableSpawnCard,
      };

      DirectorAPI.DirectorCardHolder directorCardHolder = new DirectorAPI.DirectorCardHolder
      {
        Card = directorCard,
        InteractableCategory = DirectorAPI.InteractableCategory.Shrines
      };

      DirectorAPI.Helpers.AddNewInteractable(directorCardHolder);
    }

    public class BeebleMemorialManager : NetworkBehaviour
    {
      public PurchaseInteraction purchaseInteraction;
      private GameObject shrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();

      public void Start()
      {
        if (NetworkServer.active && Run.instance)
        {
          purchaseInteraction.SetAvailable(true);
        }

        purchaseInteraction.onPurchase.AddListener(OnPurchase);
      }

      [Server]
      public void OnPurchase(Interactor interactor)
      {
        if (!NetworkServer.active)
        {
          Debug.LogWarning("[Server] function 'BeebleMemorialManager::OnPurchase(RoR2.Interactor)' called on client");
          return;
        }

        EffectManager.SpawnEffect(shrineUseEffect, new EffectData()
        {
          origin = gameObject.transform.position,
          rotation = Quaternion.identity,
          scale = 3f,
          color = Color.cyan
        }, true);
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage() { baseToken = "<style=cEvent><color=#307FFF>o7 to the Beebles we lost in the great lunar war.</color></style>" });
      }
    }

  }

}