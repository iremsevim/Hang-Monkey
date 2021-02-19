using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RuntimeData runtimeData;
    public GameData gameData;
    public readonly List<string> AlphabetWords = new List<string> {"A","B","C","D","E","F","G","H","I","J","K","L"
        ,"M","N","O","P","R","S","T","U","V","Y","X","Q","W","Z"};

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (runtimeData.isGameStarted)
        {
            runtimeData.PlayingTime += Time.deltaTime;
        }
    }

    public void AnyButtonClick()
    {
        "btnclick".PlayAudio();
    }

    public void Register()
    {
        string Username = gameData.uI.menuWindow.loginWindow.inputFieldUsername.text;
        Worker.UI.ShowHideLoadingWindow(true);
        WebApi.instance.Service(WebApi.RegisterUrl(Username)
        ,
        (string response) =>
        {
            if (response != "null" && response.Length > 0)
            {
                Debug.Log("Giris Basarıli");
                Login(response, Username);
            }
            else
            {
                Worker.UI.ShowHideErrorWindow(true, "Invalid Username");
                Debug.Log("Giris Basarısız");
            }
            Worker.UI.ShowHideLoadingWindow(false);
        },
        (string error) =>
        {
            Worker.UI.ShowHideErrorWindow(true, "Somitnig Went Wrong");
            Debug.Log(error);
            Worker.UI.ShowHideLoadingWindow(false);
        }).ExacuteIE();
    }
    public void Login(string UserID, string UserName)
    {
        runtimeData.isLoggedin = true;
        runtimeData.loginUserId = UserID;
        runtimeData.loginUserName = UserName;
        Worker.UI.UpdateMenuUserNameText(UserName);
        Worker.UI.SwitchinMenuRegisterToMain();
    }

    public void PlayGame_Menu()
    {
        Worker.UI.OpenClosedCategorySelectWindow(true);
    }
    public void ExitGame_Menu()
    {

    }
    public void CategoryToMenu()
    {
        Worker.UI.OpenClosedCategorySelectWindow(false);
    }

    public void TakeQuestion()
    {
        Worker.UI.UpdateStepText();
        runtimeData.isGameStarted = true;

        string question = runtimeData.runtimeWordList.RandomİtemSelec().ToUpper();
        runtimeData.currrentquestion = question;
        runtimeData.runtimeWordList.RemoveAll(x => x.ToUpper() == question);
        runtimeData.falsecount = 6;
        runtimeData.PlayingTime = 0;

        Worker.UI.UpdateAnswerText(question);

        runtimeData.truecount = System.Text.RegularExpressions.Regex.Replace(question, @"\s+", "").Length;

        runtimeData.scoreCurrent = 0;
        Worker.UI.UpdateScore();

        runtimeData.RealKeyboards = new List<UI_KeyBoard_Key>();

        //firefighter

       // Debug.Log(question);

        foreach (Transform item in gameData.uI.inGameWindow.KeyboardCarrier)
        {
            Destroy(item.gameObject);
        }
        List<string> fakewordlist = new List<string>();
        for (int i = 0; i < question.Length; i++)
        {
            int index = i;
            if (fakewordlist.Any(x => x == question[index].ToString()) || question[index] == ' ')
            {
                continue;
            }
            fakewordlist.Add(question[index].ToString());
        }



        List<string> RealKeys = new List<string>();
        RealKeys.AddRange(fakewordlist);

        AlphabetWords.Shuffle();
        int rand = (AlphabetWords.Count) - fakewordlist.Count;
        for (int i = 0; i < rand; i++)
        {
            int index = i;
            if (fakewordlist.Any(x => x == AlphabetWords[index]))
            {
                rand++;
                continue;
            }
            fakewordlist.Add(AlphabetWords[index]);
        }
        fakewordlist.Shuffle();
        for (int i = 0; i < fakewordlist.Count; i++)
        {
            int index = i;
            GameObject createdkey = Instantiate(gameData.uI.inGameWindow.KeyboardItem, gameData.uI.inGameWindow.KeyboardCarrier);
            createdkey.GetComponent<UI_KeyBoard_Key>().SetUp(fakewordlist[index].ToString(), () =>
            {
                Debug.Log(fakewordlist[index].ToString());
                TryLetter(fakewordlist[index].ToString());
            });
            if (RealKeys.Any(x => x == fakewordlist[i]))
            {
                runtimeData.RealKeyboards.Add(createdkey.GetComponent<UI_KeyBoard_Key>());
            }
        }

        foreach (Transform item in gameData.uI.inGameWindow.QuestionCarrier)
        {
            Destroy(item.gameObject);
        }
        for (int i = 0; i < question.Length; i++)
        {
            GameObject created = Instantiate(gameData.uI.inGameWindow.QuestionItem, gameData.uI.inGameWindow.QuestionCarrier);
            created.GetComponent<QestionItem>().SetUp(question[i].ToString());

        }


        gameData.uI.inGameWindow.MonkeyBody.ForEach(x => x.color = Color.black);

        gameData.uI.inGameWindow.KeyboardCarrier.gameObject.SetActive(true);
        gameData.uI.inGameWindow.QuestionCarrier.gameObject.SetActive(true);
        gameData.uI.inGameWindow.NextButton.gameObject.SetActive(false);
        gameData.uI.inGameWindow.TryAgainButton.gameObject.SetActive(false);
        gameData.uI.inGameWindow.MenuButton.gameObject.SetActive(false);


    }

    public void TryLetter(string key)
    {
        if (!runtimeData.isGameStarted) return;

        gameData.audio.lettervoices.Find(x => x.name.ToUpper() == key)?.PlayAudio();

        if (runtimeData.currrentquestion.Any(x => x.ToString() == key))
        {
            runtimeData.scoreCurrent += 10;
            runtimeData.score += 10;
            Worker.UI.UpdateScore();

            runtimeData.RealKeyboards.RemoveAll(x => x.title.text == key);

            Debug.Log("Yanıt doğru");

            foreach (Transform item in gameData.uI.inGameWindow.QuestionCarrier)
            {
                if (item.GetComponent<QestionItem>().title.text == key)
                {
                    item.GetComponent<QestionItem>().ShowText();
                    runtimeData.truecount--;
                }
            }
            if (runtimeData.truecount <= 0)
            {
                runtimeData.TrueAnswerCount++;
                WebApi.instance.Service(WebApi.SendTimeAndScoreUrl(runtimeData.loginUserId, gameData.general.GameCode, ((int)runtimeData.PlayingTime).ToString(),
                 runtimeData.scoreCurrent.ToString()), x => Debug.Log(x), null).ExacuteIE();
                runtimeData.isGameStarted = false;
                System.Action ro = () =>
                {

                    Debug.Log("Complated yuppy");
                    if (runtimeData.inGame)
                    {
                        gameData.general.confetticomplate.Play();
                        "complate".PlayAudio();
                    }
                   
                    gameData.uI.inGameWindow.KeyboardCarrier.gameObject.SetActive(false);
                    gameData.uI.inGameWindow.QuestionCarrier.gameObject.SetActive(false);
                    if (runtimeData.runtimeWordList.Count > 0)
                    {
                        gameData.uI.inGameWindow.NextButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        gameData.uI.inGameWindow.MenuButton.gameObject.SetActive(true);
                        Debug.Log("Complated all");
                        WebApi.instance.Service(WebApi.SendSuccessRateUrl(runtimeData.loginUserId, gameData.general.GameCode,runtimeData.RateofSuccess.ToString()), x => Debug.Log(x), null).ExacuteIE();
                    }


                };
                ro.TimedAction(1.5f).ExacuteIE();
            }

        }
        else
        {
            Debug.Log("Yanlış");
            runtimeData.falsecount--;


            Transform bodypart = gameData.uI.inGameWindow.MonkeyBody[runtimeData.falsecount].transform;
            gameData.uI.inGameWindow.MonkeyBody[runtimeData.falsecount].color = Color.white;
            bodypart.transform.DOScale(bodypart.transform.localScale / 2, 0.1f).OnComplete(() =>
            {
                bodypart.transform.DOScale(bodypart.transform.localScale * 2, 0.1f);
            });
            if (runtimeData.falsecount <= 0)
            {
                runtimeData.WrongAnswerCount++;

                runtimeData.score -= runtimeData.scoreCurrent;
                Worker.UI.UpdateScore();

                WebApi.instance.Service(WebApi.SendTimeOnlyUrl(runtimeData.loginUserId, gameData.general.GameCode,
                     ((int)runtimeData.PlayingTime).ToString()), x => Debug.Log(x), null).ExacuteIE();

                runtimeData.isGameStarted = false;
                Debug.Log("Game Over");
                "gameover".PlayAudio();

                gameData.uI.inGameWindow.KeyboardCarrier.gameObject.SetActive(false);
                gameData.uI.inGameWindow.QuestionCarrier.gameObject.SetActive(false);
                gameData.uI.inGameWindow.TryAgainButton.gameObject.SetActive(true);
                gameData.general.gameoverparticle.Play();
            }
        }
    }

    public void TryAgain()
    {
        runtimeData.runtimeWordList.Add(runtimeData.currrentquestion);
        TakeQuestion();
    }
    public void Next()
    {
        TakeQuestion();
    }

    public void Hint()
    {
        if (runtimeData.isGameStarted && runtimeData.hintcount > 0 && runtimeData.RealKeyboards.Count > 0)
        {
            runtimeData.hintcount--;
            Worker.UI.UpdateHintText();
            runtimeData.RealKeyboards.RandomİtemSelec().Clicked();
        }
    }

    public void MenuButton_InGame()
    {
        Worker.UI.InGameToMenu();
        WebApi.instance.Service(WebApi.SendTimeOnlyUrl(runtimeData.loginUserId, gameData.general.GameCode,
        ((int)runtimeData.PlayingTime).ToString()), x => Debug.Log(x), null).ExacuteIE();
    }

    public void SoundOnOff(bool Status)
    {
        AudioListener.volume = Status ? 1 : 0;
        gameData.uI.SoundOffButton.SetActive(Status);
        gameData.uI.SoundOnButton.SetActive(!Status);
    }

    private void OnApplicationQuit()
    {
        WebApi.instance.Service(WebApi.SendTimeOnlyUrl(runtimeData.loginUserId, gameData.general.GameCode,
                      ((int)runtimeData.PlayingTime).ToString()), x=>Debug.Log(x), null).ExacuteIE();
    }

}

[System.Serializable]
public struct RuntimeData
{
    public bool isLoggedin;
    public string loginUserId;
    public string loginUserName;
    public bool isGameStarted;

    public GameData.General.WordCategory selectedcategory;
    public List<string> runtimeWordList;

    public string currrentquestion;

    public int hintcount;
    public int falsecount;
    public int truecount;

    public int score;
    public int scoreCurrent;

    public List<UI_KeyBoard_Key> RealKeyboards;

    public float PlayingTime;

    public int WrongAnswerCount;
    public int TrueAnswerCount;
    public float RateofSuccess
    {
        get
        {
            float total = WrongAnswerCount + TrueAnswerCount;
            total = total == 0 ? 1 : total;
            if (TrueAnswerCount == 0) return 0;
            return ((float)(TrueAnswerCount) /total)*100;
        }
    }

    public bool inGame;

}

[System.Serializable]
public struct GameData
{
    public UI uI;
    public Audio audio;
    public General general;

    [System.Serializable]
    public struct UI
    {
        public MenuWindow menuWindow;
        public InGameWindow inGameWindow;
        public LoadingWindow loadingWindow;
        public ErrorWindow errorWindow;

        public GameObject SoundOnButton;
        public GameObject SoundOffButton;

        [System.Serializable]
        public class Window
        {
            public GameObject MainObject;
            public void Show()
            {
                MainObject.SetActive(true);
            }
            public void Hide()
            {
                MainObject.SetActive(false);
            }
        }

        [System.Serializable]
        public class MenuWindow : Window
        {
            public LoginWindow loginWindow;
            public MainWindow mainWindow;

            [System.Serializable]
            public class LoginWindow:Window
            {
                public InputField inputFieldUsername;
            }
            [System.Serializable]
            public class MainWindow : Window
            {
                public CategorySelectWindow categorySelectWindow;
                public Text userNameText;

                [System.Serializable]
                public class CategorySelectWindow:Window
                {
                    public Transform carrier;
                    public GameObject prefab;
                }
            }
        }
        [System.Serializable]
        public class InGameWindow : Window
        {
            public Transform KeyboardCarrier;
            public GameObject KeyboardItem;

            public Transform QuestionCarrier;
            public GameObject QuestionItem;

            public List<Image> MonkeyBody;

            public Text CategoryTitle;
            public Text ScoreText;
            public Text StepText;
            public Text HintText;
            public Text TextAnswer;

            public GameObject NextButton;
            public GameObject TryAgainButton;
            public GameObject MenuButton;



        }
        [System.Serializable]
        public class LoadingWindow : Window
        {

        }

        [System.Serializable]
        public class ErrorWindow:Window
        {
            public Text ErrorText;
        }
    }

    [System.Serializable]
    public struct Audio
    {
        public AudioSource MainSource;
        public List<AudioProfile> audioProfiles;
        public List<AudioClip> lettervoices;

        [System.Serializable]
        public struct AudioProfile
        {
            public string Id;
            public AudioClip Sound;
        }
    }

    [System.Serializable]
    public struct General
    {
        public string GameCode;
        public List<WordCategory> wordCategories;
        public ParticleSystem confetticomplate;
        public ParticleSystem gameoverparticle;

        public GameObject Loader;
        public Transform Loadermask;

        [System.Serializable]
        public class WordCategory
        {
            public string Title;
            public List<string> words;
          
        }
    }
     
      
}

public static class Worker
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T RandomİtemSelec<T>(this IList<T> list)
    {
        list.Shuffle();
        return list[Random.Range(0, list.Count)];
    }

    public static void ExacuteIE(this IEnumerator ie)
    {
        GameManager.instance.StartCoroutine(ie);
    }

    public static void PlayAudio(this string Id)
    {
        GameData.Audio.AudioProfile profile = GameManager.instance.gameData.audio.audioProfiles.Find(x => x.Id == Id);
        GameManager.instance.gameData.audio.MainSource.PlayOneShot(profile.Sound,1f);
    }
    public static void PlayAudio(this AudioClip Id)
    {
        GameManager.instance.gameData.audio.MainSource.PlayOneShot(Id, 1f);
    }

    public static void LoadGame(this GameData.General.WordCategory category)
    {
        Debug.Log(category.Title + "yükleniyor");
        "startgame".PlayAudio();
        GameManager.instance.runtimeData.selectedcategory = category;
        GameManager.instance.runtimeData.runtimeWordList = new List<string>();
        GameManager.instance.runtimeData.runtimeWordList.AddRange(category.words);
        GameManager.instance.runtimeData.runtimeWordList.ForEach(x =>x=x.ToUpper());

        GameManager.instance.runtimeData.WrongAnswerCount = 0;
        GameManager.instance.runtimeData.TrueAnswerCount = 0;
        GameManager.instance.runtimeData.PlayingTime = 0;

        GameManager.instance.runtimeData.hintcount = 2;
        UI.UpdateHintText();

        UI.MenuToInGame();
        GameManager.instance.TakeQuestion();

        GameManager.instance.gameData.general.Loader.SetActive(true);
        GameManager.instance.gameData.general.Loadermask.transform.localScale = Vector3.zero;
        GameManager.instance.gameData.general.Loadermask.DOScale(new Vector3(1.3f, 2.1f, 1), 1f).OnComplete(() =>
        {
            GameManager.instance.gameData.general.Loader.SetActive(false);
        });
    }

    public static IEnumerator TimedAction(this System.Action x,float time)
    {
        yield return new WaitForSeconds(time);
        x?.Invoke();
    }

    public static class UI
    {
        public static GameData.UI uIDatas
        {
            get { return GameManager.instance.gameData.uI; }
        }

        public static void ShowHideLoadingWindow(bool Status)
        {
            if (Status)
            {
               uIDatas.loadingWindow.Show();
            }
            else
            {
                uIDatas.loadingWindow.Hide();
            }
        }

        public static void SwitchinMenuRegisterToMain()
        {
            uIDatas.menuWindow.loginWindow.Hide();
            uIDatas.menuWindow.mainWindow.Show();
        }

        public static void OpenClosedCategorySelectWindow(bool status)
        {
            if(status)
            {
                uIDatas.menuWindow.mainWindow.categorySelectWindow.Show();
                foreach (Transform item in uIDatas.menuWindow.mainWindow.categorySelectWindow.carrier)
                {
                    GameManager.Destroy(item.gameObject);
                }

                foreach (var item in GameManager.instance.gameData.general.wordCategories)
                {
                    GameObject createdcategory = GameManager.Instantiate(uIDatas.menuWindow.mainWindow.categorySelectWindow.prefab, uIDatas.menuWindow.mainWindow.categorySelectWindow.carrier);
                    createdcategory.GetComponent<UI_CategorySelectButton>().SetUp(item.Title, () =>
                    {
                        item.LoadGame();
                    });
                }
            }
            else
            {
                uIDatas.menuWindow.mainWindow.categorySelectWindow.Hide();
            }
        }

        public static void UpdateMenuUserNameText(string text)
        {
            uIDatas.menuWindow.mainWindow.userNameText.text = text;
        }

        public static void MenuToInGame()
        {
            uIDatas.menuWindow.Hide();
            uIDatas.inGameWindow.Show();

            uIDatas.inGameWindow.CategoryTitle.text = GameManager.instance.runtimeData.selectedcategory.Title;

            GameManager.instance.runtimeData.inGame = true;
        }
        public static void InGameToMenu()
        {
            uIDatas.menuWindow.Show();
            uIDatas.inGameWindow.Hide();
            GameManager.instance.runtimeData.inGame = false;
        }

        public static void UpdateScore()
        {
            uIDatas.inGameWindow.ScoreText.text ="X "+ GameManager.instance.runtimeData.score.ToString();
            uIDatas.inGameWindow.ScoreText.transform.DOScale(uIDatas.inGameWindow.ScoreText.transform.localScale * 1.5f, 0.1f).OnComplete(() =>
            {
                uIDatas.inGameWindow.ScoreText.transform.DOScale(uIDatas.inGameWindow.ScoreText.transform.localScale / 1.5f, 0.1f);
            });
        }

        public static void UpdateHintText()
        {
            uIDatas.inGameWindow.HintText.text = "X " + GameManager.instance.runtimeData.hintcount.ToString();
         
        }

        public static void UpdateStepText()
        {
            int total = GameManager.instance.runtimeData.selectedcategory.words.Count;
            int last = GameManager.instance.runtimeData.runtimeWordList.Count;
            uIDatas.inGameWindow.StepText.text = ((total-last)+1) + " / " + total;
        }

        public static void UpdateAnswerText(string text)
        {
            uIDatas.inGameWindow.TextAnswer.text = "";
            for (int i = 0; i < text.Length; i++)
            {
                uIDatas.inGameWindow.TextAnswer.text += text[i] + " ";
            }
        }

       
        public static void ShowHideErrorWindow(bool status,string Title="")
        {
            uIDatas.errorWindow.ErrorText.text = Title;
            if (status)
            {
                uIDatas.errorWindow.Show();
            }
            else
            {
                uIDatas.errorWindow.Hide();
            }
            
        }
    }

}