using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class MenuPrincipalMenager : MonoBehaviour
{
    [SerializeField]
    private string nomeDoLevelDeJogo;
    //Informar porta onde Arduino esta conectado
    SerialPort serialPort;
    private const string namePort = "COM13";
    private const int baudRate = 9600;

    public int valorX = 0;
    public int valorY = 0;
    public int contador = 0;
    public bool calibrar = false;

    public TextMeshProUGUI texto;
    public TMP_Dropdown dropDown;

    private const string URL_QUERY_API = "https://localhost:7281/Patient";

    private List<Paciente> pacientes;

    private void Start()
    {
        StartCoroutine(GetRequest());
        SetarSerialPort();
    }

    private IEnumerator GetRequest()
    {
        //using UnityWebRequest webRequest = UnityWebRequest.Get(URL_QUERY_API);
        //yield return webRequest.SendWebRequest();

        //string[] pages = URL_QUERY_API.Split('/');
        //int page = pages.Length - 1;

        //switch (webRequest.result)
        //{
            //case UnityWebRequest.Result.ConnectionError:
            //case UnityWebRequest.Result.DataProcessingError:
                //Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                //break;
            //case UnityWebRequest.Result.ProtocolError:
                //Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                //break;
            //case UnityWebRequest.Result.Success:
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                //List<Paciente> pacientes = JsonConvert.DeserializeObject<List<Paciente>>(webRequest.downloadHandler.text);
                //SetarDropdown(pacientes); ;
                //break;
        //}
          using UnityWebRequest webRequest = UnityWebRequest.Get(URL_QUERY_API);
        yield return webRequest.SendWebRequest();

        string[] pages = URL_QUERY_API.Split('/');
        int page = pages.Length - 1;

        switch (webRequest.result)
        {
            //case UnityWebRequest.Result.ConnectionError:
            //case UnityWebRequest.Result.DataProcessingError:
            //    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
            //case UnityWebRequest.Result.ProtocolError:
            //    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
            case UnityWebRequest.Result.Success:
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                List<Paciente> pacientes = JsonConvert.DeserializeObject<List<Paciente>>(webRequest.downloadHandler.text);
                SetarDropdown(pacientes);
                break;
            default:
                var pacientesTeste = new List<Paciente>()
                {
                    new Paciente()
                    {
                        id = 1,
                        nome = "Paciente teste"
                    }
                };
                SetarDropdown(pacientesTeste);
                break;
        }
    }

    private void SetarDropdown(List<Paciente> pacientes)
    {
        this.pacientes = pacientes;
        dropDown.ClearOptions();
        List<string> nomePacientes = pacientes.Select(paciente => paciente.nome).ToList();
        dropDown.AddOptions(nomePacientes);
    }

    private void SetarSerialPort()
    {
        serialPort = new SerialPort(namePort, baudRate);
        serialPort.Open();
        serialPort.ReadTimeout = 50;
    }

    public void Jogar()
    {
        serialPort.Close();
        if (contador > 0 && valorX > 0 && valorY > 0)
        {
            valorX /= contador;
            valorY /= contador;
        }

        PlayerPrefs.SetInt("x", valorX);
        PlayerPrefs.SetInt("y", valorY);
        PlayerPrefs.SetInt("idNomeSelecionado", ObterIdPacienteSelecionado());
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void Update()
    {
        if (calibrar)
        {
            (int x, int y) = GetInputArduino();
            if (x > 0 && y > 0)
            {
                valorX += x;
                valorY += y;
                contador++;
            }
        }
    }
    
    private int ObterIdPacienteSelecionado()
    {
        int index = dropDown.value;
        string name = dropDown.options[index].text;
        int idNomeSelecionado = pacientes.First(pacientes => pacientes.nome.ToUpper().Equals(name.ToUpper())).id;
        return idNomeSelecionado;
    }

    public void Calibrar()
    {
        calibrar = !calibrar;
        
        string nomeTexto = "CALIBRAR";

        if (calibrar)
        {
            contador = 0;
            valorX = 0;
            valorY = 0;
            nomeTexto = "CALIBRANDO";
        }

        texto.text = nomeTexto;
    }

    public (int, int) GetInputArduino()
    {
        int x = 0;
        int y = 0;
        if (serialPort.IsOpen)
        {
            try
            {
                var data = serialPort.ReadLine();
                string[] tokens = data.Split(',');
                x = Convert.ToInt32(tokens[0]);
                y = Convert.ToInt32(tokens[1]);
                print("X: " + x + " Y: " + y);
            }
            catch (Exception ex)
            {
                Debug.Log("error" + ex.Message);
            }
        }

        return (x, y);
    }
}

public class Paciente{
    public int id;
    public string nome;
}
