using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Procedural : MonoBehaviour
{
    [Header("Nombre de pattes")]
    public Transform[] legTargets;

    [Header("Attributs du step")]
    public float stepSize = 1f;
    public float stepHeight = 0.1f;
    public float smoothness = 1f;

    //Limite la taille du raycast
    private float raycastRange = 1f;

    //État des pattes
    private int nbLeg;
    private Vector3[] initLegPosition;
    private Vector3[] lastLegPosition;
    private Vector3 lastBodyUp;
    private bool[] legMoving;

    //Polish de l'animation
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;
    private float velocityMultiplier = 15f;


    //Effectue un Raycast à partir de la zone où le pied doit atterrir pour avoir une position exacte
        //Détermine à quelle hauteur le pied doit atterrir
    static Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up, - up);
        
        if (Physics.Raycast(ray, out hit, 2f * halfRange))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }


    void Start()
    {
        lastBodyPos = transform.up;

        //Setup les array en fonction du nombre de pattes
        nbLeg = legTargets.Length;
        initLegPosition = new Vector3[nbLeg];
        lastLegPosition = new Vector3[nbLeg];
        legMoving = new bool[nbLeg];

        //Récupère les informations pour chaque patte
        for(int i = 0; i < nbLeg; i++)
        {
            initLegPosition[i] = legTargets[i].localPosition;
            lastLegPosition[i] = legTargets[i].position;
            legMoving[i] = false;
        }

        lastBodyPos = transform.position;
    }

    //Effectue le mouvement du pas (pour le pied concerné)
        //L'attribut "index" indique quel pied bouger
    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        //Indique d'où part le mouvement
        Vector3 startPos = lastLegPosition[index];

        for(int i = 1; i <= smoothness; ++i)
        {
            //Indique a quelle hauteur se lève le pied lors du pas
            legTargets[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;

            //Indique la position de la target sur le plan horizontal
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));            

            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPosition[index] = legTargets[index].position;
        legMoving[0] = false;
    }

    void FixedUpdate()
    {      
        //Polish de l'animation en fonction de la vélocité actuelle
        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);
        if (velocity.magnitude < 0.000025f)
        {
            velocity = lastVelocity;
        }
        else
        {
            lastVelocity = velocity;
        }

        Vector3[] desiredPositions = new Vector3[nbLeg];
        int legToMove = -1;
        float maxDistance = stepSize;

        //Pour chaque pied...
        for (int i = 0; i < nbLeg; ++i)
        {
            //...Traduit la position locale du pied en position globale
            desiredPositions[i] = transform.TransformPoint(initLegPosition[i]);

            //...Calcul la distance entre la position que le pied devrait avoir et sa position actuelle
            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPosition[i], transform.up).magnitude;
            
            //Défini quel pied est le plus loin de sa position initiale
            if (distance > maxDistance)
            {
                maxDistance = distance;
                legToMove = i;
            }
        }

        //S'assure que seul 1 pied est bougé à la fois
        for (int i = 0; i < nbLeg; ++i)
        {
            if (i != legToMove)
            {
                legTargets[i].position = lastLegPosition[i];
            }
        }

        //Prépare le mouvement du pied
        if (legToMove != -1 && !legMoving[0])
        {
            //Défini la zone où le pied va atterir
                //La valeur est calculée avec la position qu'il doit avoir et la vélocité
            Vector3 targetPoint = desiredPositions[legToMove] + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) * (desiredPositions[legToMove] - legTargets[legToMove].position) + velocity * velocityMultiplier;
            Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, transform.up);

            legMoving[0] = true;
            //Lance le mouvement du pied
            StartCoroutine(PerformStep(legToMove, positionAndNormal[0]));
        }

        lastBodyPos = transform.position;
    }

    //Visualisation des différents éléments de l'animation
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLeg; ++i)
        {
            //Position du pied
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            //Safe zone (le pied ne bougera pas tant qu'il est là)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(initLegPosition[i]), stepSize);
        }
    }
}
