using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    private Combatant combatant;
    [SerializeField]private Canvas healthbarCanvas;
    [SerializeField]private Image healthbarBackground;
    [SerializeField]private Image healthbarImage;
    [SerializeField]private Slider healthbarSlider;
    private Color noHealthColor = Color.red;
    private Color fullHealthColor = Color.green;

    /* *** Monobehaviour Functions *** */
    private void Awake()
    {
        if (healthbarCanvas == null || healthbarBackground == null || healthbarImage == null || healthbarSlider == null)
        {
            Debug.LogError("Healthbar components not set on " + gameObject.name + ". Destroying self.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set position to 10000, 10000, 10000 to hide this unless a combatant is already assigned
        if (combatant == null) transform.SetLocalPositionAndRotation(new Vector3(10000.0f, 10000.0f, 10000.0f), Quaternion.identity);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        CombatManager.Instance.GenerateNewRoom.AddListener(GenerateNewRoom);
    }

    private void OnDestroy()
    {
        // Destroy the healthbar canvas and components
        Destroy(healthbarCanvas.gameObject);
        Destroy(healthbarBackground.gameObject);
        Destroy(healthbarImage.gameObject);
        CombatManager.Instance.CombatEnded.RemoveListener(OnCombatEnd);
        CombatManager.Instance.GenerateNewRoom.RemoveListener(GenerateNewRoom);
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);
    }

    /* *** Sync method *** */
    public void SyncToCombatant(Combatant combatant)
    {
        if (combatant == null) Destroy(this.gameObject); // If we set combatant to null then we annihilate this
        this.combatant = combatant;
        transform.SetLocalPositionAndRotation(new Vector3(0.0f, 1.4f, 0.0f), Quaternion.Euler(45.0f, 45.0f, 0.0f));
        CombatManager.Instance.CombatantDied.AddListener(OnCombatantDeath); // Listen for this combatant to die
        CombatManager.Instance.CombatantAttacked.AddListener(UpdateHealth);
    }

    /* *** Event Handlers *** */
    private void OnCombatantDeath(Combatant combatant, Damage _)
    {
        if (this.combatant != combatant) return;

        Destroy(this.gameObject);
    }

    private void UpdateHealth(Combatant combatant, Damage _)
    {
        if (this.combatant != combatant) return;
        float healthbarVal = combatant.remainingHealth / combatant.MaxHealth;
        healthbarSlider.value = healthbarVal;
        healthbarImage.color = Color.Lerp(noHealthColor, fullHealthColor, healthbarVal);
    }

    private void OnCombatEnd()
    {
        Destroy(this.gameObject);
    }

    private void GenerateNewRoom()
    {
        Destroy(this.gameObject);
    }
}