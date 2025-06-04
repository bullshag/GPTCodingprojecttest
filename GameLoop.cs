using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class GameLoop : MonoBehaviour
{
    public Button createMatchButton;
    public Button joinMatchButton;
    public Image qrCodeImage;
    public Text countdownText;
    public Image background;
    public Text resultText;
    public Text connectionInfoText;
    public float matchDuration = 5f;

    private float _timer;
    private int _myScore;
    private int _opponentScore;
    private bool _matchRunning;
    private bool _isHost;

    private TcpListener _server;
    private TcpClient _client;
    private NetworkStream _stream;

    void Start()
    {
        createMatchButton.onClick.AddListener(OnCreateMatch);
        joinMatchButton.onClick.AddListener(OnJoinMatch);
        ShowStartMenu();
    }

    void ShowStartMenu()
    {
        createMatchButton.gameObject.SetActive(true);
        joinMatchButton.gameObject.SetActive(true);
        qrCodeImage.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        connectionInfoText.gameObject.SetActive(false);
        background.color = Color.white;
        _matchRunning = false;
        if (_stream != null)
        {
            _stream.Close();
            _stream = null;
        }
        if (_client != null)
        {
            _client.Close();
            _client = null;
        }
        if (_server != null)
        {
            _server.Stop();
            _server = null;
        }
    }

    void OnCreateMatch()
    {
        createMatchButton.gameObject.SetActive(false);
        joinMatchButton.gameObject.SetActive(false);
        qrCodeImage.gameObject.SetActive(true);
        connectionInfoText.gameObject.SetActive(true);
        StartHost();
    }

    void OnJoinMatch()
    {
        createMatchButton.gameObject.SetActive(false);
        joinMatchButton.gameObject.SetActive(false);
        connectionInfoText.gameObject.SetActive(true);
        qrCodeImage.gameObject.SetActive(false);
        // In a real game this would open the camera to scan a QR code.
        // Here we expect connectionInfoText to contain "ip:port" of the host.
        ConnectToHost(connectionInfoText.text);
    }

    async void StartHost()
    {
        _isHost = true;
        string ip = GetLocalIPAddress();
        int port = 7777;
        connectionInfoText.text = $"{ip}:{port}";

        _server = new TcpListener(IPAddress.Any, port);
        _server.Start();
        connectionInfoText.text += "\nWaiting for player...";

        _client = await _server.AcceptTcpClientAsync();
        _stream = _client.GetStream();
        ListenForMessages();
        StartCoroutine(Countdown());
    }

    async void ConnectToHost(string hostInfo)
    {
        _isHost = false;
        string[] parts = hostInfo.Split(':');
        if (parts.Length != 2)
        {
            connectionInfoText.text = "Invalid address";
            return;
        }
        string host = parts[0];
        int port;
        if (!int.TryParse(parts[1], out port))
        {
            connectionInfoText.text = "Invalid port";
            return;
        }

        _client = new TcpClient();
        await _client.ConnectAsync(host, port);
        _stream = _client.GetStream();
        ListenForMessages();
        connectionInfoText.text = "Connected!";
        StartCoroutine(Countdown());
    }

    async void ListenForMessages()
    {
        byte[] buffer = new byte[1];
        while (_client != null && _client.Connected)
        {
            int read = 0;
            try
            {
                read = await _stream.ReadAsync(buffer, 0, 1);
            }
            catch
            {
                break;
            }
            if (read > 0)
            {
                char msg = (char)buffer[0];
                if (msg == 'T')
                    OnReceiveOpponentTap();
                else if (msg == 'E')
                    EndMatch();
            }
        }
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "Go!";
        StartMatch();
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    void StartMatch()
    {
        _timer = matchDuration;
        _myScore = 0;
        _opponentScore = 0;
        _matchRunning = true;
    }

    void Update()
    {
        if (!_matchRunning)
            return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            EndMatch();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _myScore++;
            SendTap();
        }

        UpdateBackground();
    }

    void SendTap()
    {
        if (_stream != null && _stream.CanWrite)
        {
            try
            {
                _stream.WriteByte((byte)'T');
            }
            catch { }
        }
    }

    public void OnReceiveOpponentTap()
    {
        _opponentScore++;
    }

    void UpdateBackground()
    {
        float total = _myScore + _opponentScore;
        float ratio = total > 0 ? (float)_myScore / total : 0.5f;
        background.color = Color.Lerp(Color.red, Color.blue, ratio);
    }

    void EndMatch()
    {
        _matchRunning = false;
        resultText.gameObject.SetActive(true);
        if (_myScore > _opponentScore)
            resultText.text = "You Win!";
        else if (_myScore < _opponentScore)
            resultText.text = "You Lose!";
        else
            resultText.text = "Draw";
        if (_stream != null && _stream.CanWrite)
        {
            try
            {
                _stream.WriteByte((byte)'E');
            }
            catch { }
        }
        Invoke(nameof(ShowStartMenu), 2f);
    }
}
