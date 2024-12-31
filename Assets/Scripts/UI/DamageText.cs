using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;
    public Damage damage;
    public Combatant combatant;

    [SerializeField] private Color badDamageColor = Color.grey;
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color criticalDamageColor = Color.yellow;
    [SerializeField] private Color largeCriticalDamageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;

    private void Awake()
    {
        // If the TMP doesn't exist, raise an error and destroy
        if (!TryGetComponent<TextMeshPro>(out text))
        {
            Debug.LogError("No TextMeshProUGUI component found on DamageText object.");
            Destroy(this);
        }
    }

    public void SetDamageDisplay()
    {
        if (damage == null)
        {
            text.text = "0";
            text.color = badDamageColor;
            return;
        }
        else if (damage.dodgedAttack)
        {
            text.text = "Dodged!";
            text.color = badDamageColor;
            return;
        }
        DamageModifierTable resistances = combatant.GetDamageModifiers();
        float effectiveDamage = 0.0f;
        foreach (DamageType damageType in damage.damages.Keys)
        {
            if (damageType == DamageType.True) effectiveDamage += damage.damages[damageType];
            else
            {
                // Get the resistance multiplier for this damage type
                effectiveDamage += damage.damages[damageType] * resistances.GetDamageModifierByType(damageType);
            }
        }
        int displayDamage = Math.Abs((int)effectiveDamage);
        text.text = displayDamage.ToString();
        if (effectiveDamage == 0.0f)
        {
            text.color = badDamageColor;
        }
        else if (effectiveDamage < 0.0f)
        {
            text.color = healColor;
        }
        else if (damage.critCount > 0)
        {
            if (damage.critCount == 1)
            {
                text.color = criticalDamageColor;
            }
            else
            {
                text.color = largeCriticalDamageColor;
            }
        }
        else
        {
            text.color = normalDamageColor;
        }
        return;
    }

    public IEnumerator DisplayDamageText()
    {
        SetDamageDisplay();
        transform.position = combatant.transform.position + new Vector3(0, 0.75f, 0); // Position self above the combatant
        float time = 0.0f;
        while (time < 10.0f)
        {
            time += Time.deltaTime;
            float lerpProgress = time / 10.0f;
            transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, 0.25f, 0), lerpProgress);
            text.color = Color.Lerp(text.color, new Color(text.color.r, text.color.g, text.color.b, 0), lerpProgress);
            yield return null;
        }
        Destroy(gameObject); // Destroy self once done
    }

    void Start()
    {
        transform.rotation = Camera.main.transform.rotation; // Make sure the text is always facing the camera
        StartCoroutine(DisplayDamageText());
    }
}
