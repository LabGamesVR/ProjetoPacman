using Assets.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const string URL_COMMAND_API = "https://localhost:7098/";

    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public Text gameOverText;
    public Text scoreText;
    public Text livesText;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }

    public int quantidadeGhosts;

    private void Start()
    {
        Debug.Log("------------");
        Debug.Log("INICIO DO GAME");
        Debug.Log("---------");
        quantidadeGhosts = 0;
        NewGame();
    }

    private void Update()
    {
        if (lives > 0 && quantidadeGhosts < 4 && (Input.GetKeyDown(KeyCode.Space)))
        {
            quantidadeGhosts += 1;
            NewGame(false);
        }

        if (lives <= 0 && Input.anyKeyDown)
        {
            quantidadeGhosts = 0;
            NewGame();
        }
    }

    private void NewGame(bool resetScore = true)
    {
        if (resetScore)
            SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        gameOverText.enabled = false;

        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetState();
        }

        for (int i = ghosts.Length - 1; i >= quantidadeGhosts; i--)
        {
            ghosts[i].Desativar();
        }

        pacman.ResetState();
    }

    private void GameOver()
    {
        gameOverText.enabled = true;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);

        StartCoroutine(PostRequest());
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        pacman.DeathSequence();

        SetLives(lives - 1);

        if (lives > 0)
        {
            Invoke(nameof(ResetState), 3f);
        }
        else
        {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private IEnumerator PostRequest()
    {
        int idPaciente = PlayerPrefs.GetInt("idNomeSelecionado");

        var game = pacman.game;
        game.PatientId = idPaciente;
        game.Fim = DateTime.Now;

        var jsonDataToSend = JsonConvert.SerializeObject(game);
        print("POST ENVIADO: " + jsonDataToSend);

        using UnityWebRequest webRequest = new UnityWebRequest(url: $"{URL_COMMAND_API}Game", method: "POST");
        webRequest.SetRequestHeader(name: "Content-Type", value: "application/json");
        byte[] data = Encoding.UTF8.GetBytes(jsonDataToSend);
        webRequest.uploadHandler = new UploadHandlerRaw(data);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.InProgress:
                Debug.Log("InProgress");
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("Success");
                break;
            case UnityWebRequest.Result.ConnectionError:
                Debug.Log("ConnectionError");
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.Log("ProtocolError");
                break;
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("DataProcessingError");
                break;
            default:
                break;
        }

        pacman.game = new();
    }

}
