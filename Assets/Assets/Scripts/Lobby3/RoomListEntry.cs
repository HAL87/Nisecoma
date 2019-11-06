using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Button))]
public class RoomListEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameLabel = default;
    [SerializeField]
    private TextMeshProUGUI messageLabel = default;
    [SerializeField]
    private TextMeshProUGUI playerCounter = default;

    private RectTransform rectTransform;
    private Button button;
    private string roomName;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        // リスト要素がクリックされたら、対応したルーム名のルームに参加する
        button.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomName));
    }

    public void Activate(RoomInfo info)
    {
        roomName = info.Name;

        nameLabel.text = (string)info.CustomProperties["DisplayName"];
        messageLabel.text = (string)info.CustomProperties["Message"];
        playerCounter.SetText("{0}/{1}", info.PlayerCount, info.MaxPlayers);
        // ルームの参加人数が満員でない時だけ、クリックできるようにする
        button.interactable = (info.PlayerCount < info.MaxPlayers);

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public RoomListEntry SetAsLastSibling()
    {
        rectTransform.SetAsLastSibling();
        return this;
    }
}
