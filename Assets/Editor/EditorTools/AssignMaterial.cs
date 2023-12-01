using UnityEditor;
using UnityEngine;
using System.Collections;

public class AssignMaterial : ScriptableWizard {

// an editor script to keep me sane.

    public Material material_to_apply;

    void OnWizardUpdate ()
    {
        helpString = "Select Game Objects";
        isValid = ( material_to_apply != null );
    }

    void OnWizardCreate ()
    {
        GameObject [] gameObjects = Selection.gameObjects;
        foreach( GameObject gameObject in gameObjects )
        {
            Material[] materials = gameObject.GetComponent<Renderer> ().sharedMaterials;
            for ( int i = 0 ; i < materials.Length ; i++ )
                materials [ i ] = material_to_apply;
            gameObject.GetComponent<Renderer>().sharedMaterials = materials;

            materials = gameObject.GetComponent<Renderer> ().sharedMaterials;
            for ( int i = 0 ; i < materials.Length ; i++ )
                materials [ i ] = material_to_apply;
            gameObject.GetComponent<Renderer>().sharedMaterials = materials;
        }

    }

	[MenuItem("GameObject/Assign Material", false, 4)]
    static void CreateWindow ()
    {
        ScriptableWizard.DisplayWizard ("Assign Material", typeof(AssignMaterial), "Assign");
    }
}