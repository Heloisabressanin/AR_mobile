using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ScriptableObjects/VivantConfiguration")]
public class VivantConfiguration : ScriptableObject
{
    [Header("Apparence")]
    public Vector2 tailleRandom;
    public Vector2 masseRandom;
    public List<Material> materiauxRandom = new();

    [Header("Déplacement")]
    public Vector2 rayonMouvement;   // X = min, Y = max (positif)
    public Vector2 tempsAttente;
    public float acceleration;
    public float vitesseMax;
    public float distanceArret;

    [Header("Saut")]
    public Vector2 tempsEntreSauts;
    public Vector2 forceSaut;

 public float rayonNourriture;
public float distanceManger;
public float nourrirGrossissement;

    public float yDespawn;
}
