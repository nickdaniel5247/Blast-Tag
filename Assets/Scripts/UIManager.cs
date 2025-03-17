using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public GameObject MainMenu;
    public GameObject InGame;
    public AudioClip button;
    public AudioClip button2;

    private InputSystem controls;
    private GameManager gameManager;

    public void OnCreateLobby()
    {
        NetworkManager.Singleton.StartHost();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(button);
        MainMenu.SetActive(false);

        PresentStartGame();
    }

    public void OnJoinLobby()
    {
        NetworkManager.Singleton.StartClient();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(button2);
        MainMenu.SetActive(false);

        PresentStartGame();
    }

    public void PresentStartGame()
    {
        if (IsServer)
        {
            InGame.SetActive(true);
        }
    }

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();   
        controls = new InputSystem();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        if (InGame.activeSelf && controls.Player.Attack.IsPressed())
        {
            gameManager.StartGame();
            InGame.SetActive(false);
        }
    }
}
