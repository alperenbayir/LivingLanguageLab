using UnityEngine;
using System.Collections.Generic;

public class WordItem : MonoBehaviour
{
    [Header("Debug Info (Read Only)")]
    public string objectID; 
    [HideInInspector] public string germanWord;
    
    [Header("Hover Highlight")]
    public bool enableHighlight = true;
    public Color hoverColor = new Color(0.3f, 0.8f, 1f, 1f); // Cyan - works on light and dark objects
    
    private bool isHighlighted = false;
    private Renderer[] allRenderers;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    void Start()
    {
        // Get the name of this GameObject 
        string rawName = gameObject.name;

        // Clean the GameObject name
        objectID = rawName.Split('(')[0].Trim();

        // Ask Manager for data
        if (VocabularyManager.Instance != null)
        {
            ItemData data = VocabularyManager.Instance.GetItem(objectID);

            if (data != null)
            {
                germanWord = data.german;
            }
            else
            {
                Debug.LogError("JSON MISSING: Could not find ID '" + objectID + "' for object '" + gameObject.name + "'");
            }
        }
        
        // Setup highlight materials
        SetupHighlightMaterial();
    }
    
    void SetupHighlightMaterial()
    {
        if (!enableHighlight) return;
        
        // Get ALL renderers in children
        allRenderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // Store original material
                originalMaterials[renderer] = renderer.sharedMaterial;
                
                // Create material instance
                renderer.material = new Material(renderer.sharedMaterial);
            }
        }
    }
    
    /// <summary>
    /// Call this when player hovers over the object
    /// </summary>
    public void OnHoverStart()
    {
        if (!enableHighlight || allRenderers == null) return;
        
        isHighlighted = true;
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Blend with highlight color
                renderer.material.color = hoverColor;
            }
        }
    }
    
    /// <summary>
    /// Call this when player stops hovering
    /// </summary>
    public void OnHoverEnd()
    {
        if (!enableHighlight || allRenderers == null) return;
        
        isHighlighted = false;
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Restore original material
                if (originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = new Material(originalMaterials[renderer]);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Cleanup material instances
        if (allRenderers != null)
        {
            foreach (Renderer renderer in allRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Destroy(renderer.material);
                }
            }
        }
    }
}
