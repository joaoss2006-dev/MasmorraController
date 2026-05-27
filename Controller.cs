using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SFB;

public class Controller : MonoBehaviour
{
    [SerializeField] private GameObject cubo;
    [SerializeField] private Material[] materiais;
    [SerializeField] private Material[] materiaisNumerados;
    [SerializeField] private Material materialBase;
    [SerializeField] private GameObject cuboDeGiroCamera;
    private GameObject[,,] cubos = new GameObject[5, 5, 5];
    private bool girando = false;
    private int direcaoGiro = 0;
    private int objetivoGiro = 0;

    [SerializeField] private Camera[] cameraPrint = new Camera[5];

    [SerializeField] GameObject camera1;
    [SerializeField] GameObject camera2; 
    [SerializeField] private GameObject[] imagemVisores;
    private int qualVisor = 0;

    [SerializeField] private GameObject[] giradorY;
    [SerializeField] private GameObject[] giradorX;
    [SerializeField] private GameObject[] giradorZ;
    private int[] preX = { 0, 0 };
    private int[] preZ = { 0, 0 };
    private int[] padroes = { 0, 90, 180, 270 };

    private Dictionary<Transform, Quaternion> rotacoesOriginais = new Dictionary<Transform, Quaternion>();

    private GameObject girador;
    private int quant = 1;

    private int vai = 1;

    private int qualMaterial = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Material RetornaMaterial()
    {
        return materiaisNumerados[qualMaterial++];
    }

    private void SalvaPDF()
    {
        RenderTexture[] rem = new RenderTexture[5];
        byte[][] pngData = new byte[5][];
        for (int i = 0; i < 5; i++)
        {
            cameraPrint[i].Render();

            rem[i] = cameraPrint[i].targetTexture;

            RenderTexture.active = rem[i];
            Texture2D tex = new Texture2D(rem[i].width, rem[i].height, TextureFormat.RGBA32, false, false);
            Rect esp = new Rect(0, 0, rem[i].width, rem[i].height);

            tex.ReadPixels(esp, 0, 0);
            tex.Apply();

            Color[] pixels = tex.GetPixels();

            for (int j = 0; j < pixels.Length; j++)
            {
                pixels[j] = pixels[j].gamma;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            pngData[i] = tex.EncodeToPNG();
        }


        RenderTexture.active = null;

        PdfDocument document = new PdfDocument();

        foreach (byte[] imageBytes in pngData)
        {
            PdfPage page = document.AddPage();

            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                XImage img = XImage.FromStream(() => ms);

                // Ajusta página ao tamanho da imagem
                page.Width = img.PixelWidth;
                page.Height = img.PixelHeight;

                gfx.DrawImage(img, 0, 0, page.Width, page.Height);
            }
        }
        var path = StandaloneFileBrowser.SaveFilePanel(
        "Salvar PDF",
        "",
        "mapa" + quant,
        "pdf"
        );
        quant++;

        if (!string.IsNullOrEmpty(path))
        {
            document.Save(path);
        }
    }

    void Start()
    {
       
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.A))
        {
            cuboDeGiroCamera.transform.Rotate(0, 70 * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            cuboDeGiroCamera.transform.Rotate(0, -70 * Time.deltaTime, 0);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SalvaPDF();
        }


        if (girando)
        {

            if (girador.CompareTag("Y"))
            {
                girador.transform.Rotate(0, direcaoGiro * Time.deltaTime, 0);
            }
            else if (girador.CompareTag("X"))
            {
                girador.transform.Rotate(direcaoGiro * Time.deltaTime, 0, 0);
            }
            else if (girador.CompareTag("Z"))
            {
                girador.transform.Rotate(0, 0, direcaoGiro * Time.deltaTime);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && qualVisor < 5)
        {
            foreach(GameObject obj in imagemVisores)
            {
                obj.SetActive(false);
            }
            qualVisor++;
            imagemVisores[qualVisor].SetActive(true);
            ChecaVisor();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && qualVisor > 0)
        {
            foreach (GameObject obj in imagemVisores)
            {
                obj.SetActive(false);
            }
            qualVisor--;
            imagemVisores[qualVisor].SetActive(true);
            ChecaVisor();
        }
       
    }

    private void ParaDeGirar()
    {
        girando = false;
        if (girador.CompareTag("Y"))
        {
            girador.transform.eulerAngles = new Vector3(0, objetivoGiro, 0);
        }
        else if (girador.CompareTag("X"))
        {
            girador.transform.eulerAngles = new Vector3(objetivoGiro, 0, 0);
        }
        foreach (Transform filho in girador.transform)
        {
            filho.SetParent(null);

            // Corrigir posição para grid
            Vector3 pos = filho.position;
            pos.x = Mathf.Round(pos.x / 2f) * 2f;
            pos.y = Mathf.Round(pos.y / 2f) * 2f;
            pos.z = Mathf.Round(pos.z / 2f) * 2f;
            filho.position = pos;

            // Corrigir rotação para múltiplos de 90
            Vector3 rot = filho.eulerAngles;
            rot.x = Mathf.Round(rot.x / 90f) * 90f;
            rot.y = Mathf.Round(rot.y / 90f) * 90f;
            rot.z = Mathf.Round(rot.z / 90f) * 90f;
            filho.eulerAngles = rot;
        }
    }

    private void ChecaVisor()
    {
        if(qualVisor == 0)
        {
            camera2.SetActive(false);
            camera1.SetActive(true);
            
        }
        else
        {
            camera1.SetActive(false);
            camera2.SetActive(true);
            camera2.transform.position = new Vector3(4, 1+(2*(qualVisor-1)), 4);
        }
    }



    private void GiraY(int qualGirador = -1, int rand = -1)
    {
        if (qualGirador == -1) qualGirador = Random.Range(0, 2);
        giradorY[qualGirador].GetComponent<BoxCollider>().enabled = true;
        giradorY[qualGirador].GetComponent<ColizorController>().ChecaColisoes();
        girador = giradorY[qualGirador];
        girando = true;

        //Salvando as rotações dos cubos
        foreach (Transform filho in girador.transform)
        {
            rotacoesOriginais[filho] = filho.rotation;
        }

        if (rand == -1) rand = Random.Range(0, 2);
        direcaoGiro = 90;
        if (rand == 0) { direcaoGiro *= -1; }
        objetivoGiro = (int)(giradorY[qualGirador].transform.eulerAngles.y + direcaoGiro);
        Invoke("ParaDeGirar", 1f);
        giradorY[qualGirador].GetComponent<BoxCollider>().enabled = false;
    }

    private void GiraX(int qualGirador = -1, int rand = -1)
    {
        if(qualGirador == -1) qualGirador = Random.Range(0, 2);
        giradorX[qualGirador].GetComponent<BoxCollider>().enabled = true;
        giradorX[qualGirador].GetComponent<ColizorController>().ChecaColisoes();
        girador = giradorX[qualGirador];
        girando = true;

        //Salvando as rotações dos cubos
        foreach (Transform filho in girador.transform)
        {
            rotacoesOriginais[filho] = filho.rotation;
        }

        if(rand == -1) rand = Random.Range(0, 2);
        direcaoGiro = 90;
        if (rand == 0) { direcaoGiro *= -1; }
        if (direcaoGiro > 0)
        {
            preX[qualGirador]++;
            if (preX[qualGirador] > 3) { preX[qualGirador] = 0; }
        }
        else if (direcaoGiro < 0)
        {
            preX[qualGirador]--;
            if (preX[qualGirador] < 0) { preX[qualGirador] = 3; }
        }
        objetivoGiro = padroes[preX[qualGirador]];
        Invoke("ParaDeGirar", 1f);
        giradorX[qualGirador].GetComponent<BoxCollider>().enabled = false;
    }

    private void GiraZ(int qualGirador = -1, int rand = -1)
    {
        if (qualGirador == -1) qualGirador = Random.Range(0, 2);
        giradorZ[qualGirador].GetComponent<BoxCollider>().enabled = true;
        giradorZ[qualGirador].GetComponent<ColizorController>().ChecaColisoes();
        girador = giradorZ[qualGirador];
        girando = true;

        //Salvando as rotações dos cubos
        foreach (Transform filho in girador.transform)
        {
            rotacoesOriginais[filho] = filho.rotation;
        }

        if (rand == -1) rand = Random.Range(0, 2);
        direcaoGiro = 90;
        if (rand == 0) { direcaoGiro *= -1; }
        if (direcaoGiro > 0)
        {
            preZ[qualGirador]++;
            if (preZ[qualGirador] > 3) { preZ[qualGirador] = 0; }
        }
        else if (direcaoGiro < 0)
        {
            preZ[qualGirador]--;
            if (preZ[qualGirador] < 0) { preZ[qualGirador] = 3; }
        }
        objetivoGiro = padroes[preZ[qualGirador]];
        Invoke("ParaDeGirar", 1f);
        giradorZ[qualGirador].GetComponent<BoxCollider>().enabled = false;
    }

    public void Gira()
    {
        if (!girando)
        {
            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    GiraX();
                    break;
                case 1:
                    GiraY();
                    break;
                case 2:
                    GiraZ();
                    break;
            }
        }
    }





}
