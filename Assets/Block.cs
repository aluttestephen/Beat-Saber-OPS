using UnityEngine;

public class Block : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int blockType;
    public float speed = 8f;
    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        SaberController saber = other.GetComponent<SaberController>();
        if(saber == null) return; 

        bool correctPlayer = (blockType == 0 && saber.playerID == 1) || (blockType == 1 && saber.playerID == 2);

        if (correctPlayer && saber.IsSwinging())
        {
            GameManager.Instance.AddScore(100);
            Destroy(gameObject);
            BlockDestroyVFX.Instance.PlayHitEffect(transform.position, GetComponent<Renderer>().material.color);
        }
        else
        {
            GameManager.Instance.LoseLife();
            Destroy(gameObject);
            BlockDestroyVFX.Instance.PlayHitEffect(transform.position, GetComponent<Renderer>().material.color);
        }
    }
}
