using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

public class GooglePlayGamesAdaptor : MonoBehaviour
{
    [Serializable]
    public class UnityStringGPSEvent: UnityEvent<string>
    {
        //Disaridan Gelen Callbackler Icin Yanit Stringi Callback Sinifidir.
    }

    public TextMeshProUGUI log_Connection_Tmp;
    public TextMeshProUGUI log_Notification_Tmp;

    [Header("GPS Data Kaydi Bittiginde Callback Cagir.")]
    public UnityStringGPSEvent OnWriteDataComplete;

    void Start()
    {
        SignAutomaticallyAsync();
    }

    #region Oyuna Google Play Hesabi Ile Giris

    private void Update()
    {

    }

    public IEnumerator ASignAutomaticallyAsync()
    {
        //IEnumerator ile olusan server-client cevaplarindaki gecikmenin onune gecilir.
        bool response = false;
        bool isConnectedToGPS = false;
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if(success == SignInStatus.Success)
            {
                response = true;
                isConnectedToGPS = true;
            }
            else
            {
                response = true;
                isConnectedToGPS = false;
            }
        });
        yield return new WaitUntil(() => response == true);

        string connectionInformation = $"No Connection Information!";

        //Server Cevabi Sonrasi
        if(isConnectedToGPS == true)
        {
            connectionInformation = $"Connection Success!";
        }
        else
        {
            connectionInformation = $"Connection Failed!";
        }

        if(log_Connection_Tmp != null)
        {
            log_Connection_Tmp.text = connectionInformation;
        }
    }

    public void SignAutomaticallyAsync() 
    {
        //Oyuna Otomatik Giris.
        StartCoroutine(ASignAutomaticallyAsync());
    }

    public IEnumerator ASignManually_Async()
    {
        //IEnumerator ile olusan server-client cevaplarindaki gecikmenin onune gecilir.
        bool response = false;
        bool isConnectedToGPS = false;
        PlayGamesPlatform.Instance.ManuallyAuthenticate((success) =>
        {
            if(success == SignInStatus.Success)
            {
                response = true;
                isConnectedToGPS = true;
            }
            else
            {
                response = true;
                isConnectedToGPS = false;
            }
        });
        yield return new WaitUntil(() => response == true);

        string connectionInformation = $"No Connection Information!";

        //Server Cevabi Sonrasi
        if(isConnectedToGPS == true)
        {
            connectionInformation = $"Connection Success!";
        }
        else
        {
            connectionInformation = $"Connection Failed!";
        }

        if(log_Connection_Tmp != null)
        {
            log_Connection_Tmp.text = connectionInformation;
        }
    }
    
    public void SignManually_Async()
    {
        //Manuel Olarak Giris Yapmayi Dene!
        StartCoroutine(ASignManually_Async());
    }

    #endregion Oyuna Google Play Hesabi Ile Giris

    #region Leaderboard Tablosunu Kayit

    private string _scoreSavedNoticationOnSuccess = "Score Basariyla Kaydedilmistir!";
    private string _scoreSavedNoticationOnFail = "Score Kaydi Basarisiz!";
    public IEnumerator APostScoreToLeaderboard_Async(long newScore)
    {
        //Leaderboard Tablosuna Son Kaydi Insert Eder.
        //Note : Social.ReportScore => PlayGamesPlatform.Instance.ReportScore Degisimi ile Duzgun Calismaya Basladi.

        bool response = false;
        bool result = false;
        PlayGamesPlatform.Instance.ReportScore(newScore, GPGSIds.leaderboard, (success) => 
        {
            //Serverdan gelen yanit olumlu veya olumsuz bir yanittir. Bu nedenle yanit her durumda olumludur.
            //Ancak yanit icerigi olumlu veya olumsuz olabilir.
            if (success)
            {
                Debug.Log("Yeni Score Kaydi Basarili!");
                response = true;
                result = true;
            }
            else
            {
                Debug.Log("Kayit Basarisiz!");
                response = true;
                result = false;
            }
        });

        //Serverdan Olumlu veya Olumsuz Yanit Gelene Kadar Task Bekletilir. Yanit Geldiginde Sonraki Adimlara Gecer.
        yield return new WaitUntil(() => response == true);

        //Test Amacli If Condition Kullanildi!

        if(result == true)
        {
            //Basarili Kayit Durumunda Basarili Stringini Yazdir.
            log_Notification_Tmp.text = _scoreSavedNoticationOnSuccess;
        }
        else
        {
            //Basarisiz Durumunda Basarisiz Strinigini Yazdir.
            log_Notification_Tmp.text = _scoreSavedNoticationOnFail;
        }
    }

    public void PostScoreToLeaderboard_Async()
    {
        //Kullanicinin Elde Ettigi Ilgili Degeri Leaderboard Tablosunu Isler.
        StartCoroutine(APostScoreToLeaderboard_Async(250));
    }

    #endregion Leaderboard Tablosunu Kayit

    public void PostIncrementalAchievementScoreAsync()
    {
        StartCoroutine(APostIncrementalAchievementScoreAsync());
    }

    //APostIncrementalAchievementScoreAsync Taski Mesgul mu Degil mi. Sonuca Gore Tekrar Cagrilmayi Mumkun Hale Getir.
    private bool isBusy_APostIncrementalAchievementScoreAsync = false;
    public IEnumerator APostIncrementalAchievementScoreAsync()
    {
        if(isBusy_APostIncrementalAchievementScoreAsync == true)
        {
            //Zaten isleme alinmis kodun devamini calistirma!
            yield break;
        }
        else
        {
            //Mesgul Degildi, Mesgule Cek. Koda Devam Et.
            isBusy_APostIncrementalAchievementScoreAsync = true;
        }
      
        yield return AFetch_Incremental_AchievementsAsync();

        bool response = false;
        string str = "";
        for(int i = 0; i < _availableAchievements.Count; i++)
        {
            //Achievement Database Idleri.
            str = str + _availableAchievements[i].id.ToString() + "\n";
        }

        log_Notification_Tmp.text = str;

        //Eger Tamamlanmamis Bir Gorev Varsa Liste Zaten 0 dan Buyuk Olmak Zorundadir. Birden Fazla Tamamlanmamis Gorev Varsa Ilk Indexteki Gorevi Al.
        if(_availableAchievements.Count > 0)
        {
            //Eger idsi verilen achievement henuz tamamlanmamissa bu kisim icra edilir.
            SetIncrementalProgress(_availableAchievements[0].id, 200, (success) => 
            {
                if(success == true)
                {
                    //Her iki durumda da response geldi sayilir.
                    response = true;
                }
                else
                {
                    response = true;
                }
            });
        }

        yield return new WaitUntil(() => response == true);
        //Surec Sona Erdi. Task Tamamlandi Artil Mesgul Degil ve Tekrar Cagrilabilir.
        isBusy_APostIncrementalAchievementScoreAsync = false;
    }

    //Henuz Tamamlanmamis Achievementlarin Idlerinin Listesini Dondurur.
    private List<IAchievement> _availableAchievements;
    public IEnumerator AFetch_Incremental_AchievementsAsync() 
    {
        //Static Tanimlandigindan Static Metodlarda da Cagrilabilir. IAchievements[] array donuyor. Eger Baglanti Koparsa Array Null Doner mi?
        bool response = false;
        PlayGamesPlatform.Instance.LoadAchievements((achievements) =>
        {
            //Achievements Tum Dbleri Getir. Basarisi Tamamlananlar icin Kodun Geri Kalanini Calistirma.
            if(achievements != null)
            {
                response = true;
                _availableAchievements = achievements.Where(x => x.completed == false).ToList();
            }
            else
            {
                response = true;
            }       
        });

        yield return new WaitUntil(() => response == true);
    }

    public static void SetIncrementalProgress(string gpsdId, int incrementalScore, Action<bool> actionBool)
    {
        //Verilen Incremental Achievement Tablosunda Belirtilen Miktarda Artim Saglar.
        //Action bool Ile Belirlenen Parametre Operasyon Basarili mi Degil mi Belirler.
        PlayGamesPlatform.Instance.IncrementAchievement(gpsdId, incrementalScore, actionBool);
    }

    public void ShowLeaderboard()
    {
        //Leaderboard Tablosunu Kullaniciya Gosterir.
        PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard);
    }

    public void ShowAchievements()
    {
        //Achievements UI Goster. PlayGamesPlatform Login Kullanildiginda Show Islemini de Ayni Yontemle Sagla.
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }


    #region Incremental Achievement Tablolarinin Id Verilerini Getirir.
    public void ResetAchievementsAsync()
    {
        StartCoroutine(AResetAchievementsAsync());
    }

    public IEnumerator AResetAchievementsAsync()
    {
        string gpsdId = "";
        bool response = false;
        PlayGamesPlatform.Instance.LoadAchievements((achievements) =>
        {
            //Achievements Tum Dbleri Getir. Basarisi Tamamlananlar icin Kodun Geri Kalanini Calistirma. 
            if (achievements != null)
            {
                foreach(IAchievement a in achievements)
                {
                    gpsdId += a.id + "\n";
                }
                response = true;
            }
            else
            {
                gpsdId = "No Database Found";
                response = true;
            }
        });

        yield return new WaitUntil(() => response == true);
        log_Notification_Tmp.text = gpsdId;
    }

    #endregion Incremental Achievement Tablolarinin Id Verilerini Getirir.


    #region Google Cloud Veri Yazma Metodlari

    [Serializable]
    public class Inventory
    {
        public string name;
        public bool isMale;
        public int level;
        public List<string> items;
    }

    [SerializeField] Inventory inventory;

    public byte[] TranslateDataToBytes(object anyObject)
    {
        //Verilen T Nesne Objesini JSON Stringine Donusturur. Donusen Stringi Byte Arraylerine Donusturulur.
        //Zaten JSON Bir Obje Referansi Alir. Kisitlama ve Soyutlama En Ust Seviyededir.
        string jsonData = JsonUtility.ToJson(anyObject);     
        return Encoding.UTF8.GetBytes(jsonData);
    }

    public string TranslateBytesToJSON(byte[] bytes)
    {
        //Byte Arrayinden JSON Stringine Donusturulur.
        return Encoding.UTF8.GetString(bytes);
    }

    private byte[] referansBytes;
    public void PrintBytesToTMP()
    {
        //Serialize Nesneden JSON Datasina Oradan da Byte Arrayine Donusen Verileri TMP Uzerinde Yazdirilir.
        string str = string.Empty;
        byte[] dataBytes = TranslateDataToBytes(inventory);

        //Sonra Duzenle
        referansBytes = dataBytes;

        for (int i = 0; i < dataBytes.Length; i++)
        {
            str += dataBytes[i].ToString();
        }
        log_Notification_Tmp.text = str;
    }

    public void PrintBytesToStr()
    {
        //Byte Arrayinden JSON Stringine Donusturulur ve Ekraba Yazdirilir.
        log_Notification_Tmp.text = TranslateBytesToJSON(referansBytes);
    }

    private bool isBusy_ATryOpenSavedGame_Async = false;
    public DataSource dataSourceInitial = DataSource.ReadNetworkOnly;
    public IEnumerator ATryOpenSavedGame_Async(string filename)
    {
        //Bos ve null deger geldiginde de islem yapmasin, google play service server kitlenmesine neden oluyor.
        if(filename == null || filename == "")
        {
            yield break;
        }

        //Eger halen islem devam ediyorsa IEnumeratordan cikis yap.
        if (isBusy_ATryOpenSavedGame_Async == true)
        {
            yield break;
        }
        
        //Eger islemde olan okuma veya yazma islemi yoksa devam et ve IEnumerator metodunu mesgul olarak isaretle.
        isBusy_ATryOpenSavedGame_Async = true;

        Debug.Log("ATryOpenSavedGame_Async IEnumerator Method : Parameter : " + filename); //Hata Testi LogCat Aramasinda Bulmak Icin

        //Once Google Cloud Uzerinde Kaydedilecek Dosyanin Olusturulmasi Gerek. Bu Standart Open, Read, Write Yontemleri gibi Dusunulebilir. Istemci Client Obj Dondurulur.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        //Distaki Callback icin Booleanlar.
        bool response = false;
        bool decision = false;

        //Icteki Callack icin Booleanlar.
        bool isDataSaved = false;
        bool dataSavedResponse = false;

        //Clienti Dosya Okuma ve Yazma Islemlerine Hazirlar.
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseManual, (SavedGameRequestStatus status, ISavedGameMetadata tryOpenMetaData) =>
            {
                //Her durumda server tarafindan olumlu ya da olumsuz bir cevap gelecek. Gelen yanit her durumda true olacak.
                //Olumlu yanit durumunda yani success true oldugunda decision true, success false ise decision false olacaktir.
                if (status == SavedGameRequestStatus.Success)
                {
                    response = true;
                    decision = true;

                    //Kaydedilecek veriyi byte zincirine donustur ve yazdirmak icin bir sonraki adima gec.
                    byte[] convertedByteData = TranslateDataToBytes(inventory);       
                    WriteData_To_GoogleCloudService(tryOpenMetaData, convertedByteData, 
                        (SavedGameRequestStatus status, ISavedGameMetadata tryWriteMetaData) => 
                        { 
                            if(status == SavedGameRequestStatus.Success)
                            {
                                dataSavedResponse = true;
                                isDataSaved = true;
                            }
                            else
                            {
                                dataSavedResponse = true;
                                isDataSaved = false;
                            }
                        });
                }
                else
                {
                    response = true;
                    decision = false;
                }
            });

        yield return new WaitUntil(() => { return response == true && dataSavedResponse == true; });

        //Tum surcler bittigi icin artik IEnumerator okuma yazma metodu artik bitmistir ve mesguliyeti bos olarak isaretle.
        isBusy_ATryOpenSavedGame_Async = false;

        //Sonucu Ekranda Bastirmak Amaciyla Eklenmistir.
        string notification = "Islem Kaydi Bulunamadi!";
        if(decision == true)
        {
            notification += "Dosya Okuma ve Yazma Islemleri icin Acildi!" + "\n";
            if(isDataSaved == true)
            {
                notification += $"Dosya Google Cloud Uzerine Yazildi! DataSaveResponse : {dataSavedResponse}." + "\n";
                
            }
            else
            {
                notification += $"Dosya Google Cloud Uzerine Yazma Basarisiz! : DataSaveResponse : {dataSavedResponse}." + "\n";
            }

            //Yukardan asagi gelerek son stringi olustur ve yazdir.
                    
        }
        else
        {
            notification = "Dosya Okuma ve Yazma Islemleri icin Acilamadi!";
        }

        //Eger Callback Atanmissa Calistir.
        OnWriteDataComplete?.Invoke(notification);

        if(log_Notification_Tmp != null)
        {
            log_Notification_Tmp.text = notification;
        }
        else
        {
            Debug.LogError("Notification Text Alani Referansi Null!");
        }
    }

    public void TryOpenSavedGame_Async(string savedFileName)
    {
        //Dosyayi Okuma ve Yazmaya Hazirlayan Coroutine Metodu.
        Debug.Log("TryOpenSavedGame_Async : Parameter : " + savedFileName);
        StartCoroutine(ATryOpenSavedGame_Async(savedFileName));
    }

    public void WriteData_To_GoogleCloudService(ISavedGameMetadata comingFromOpenGameMetaData, byte[] savedData, Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        //Bu metodun calismasi icin once server uzerinde dosya open metodunun calistirilmasi gerekir. Bu metod onun callback metodudur.
        //Once Builder Objesini Olusturulur ve Builder Configleri Ayarlanir.
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
            .WithUpdatedPlayedTime(TimeSpan.FromMinutes(comingFromOpenGameMetaData.TotalTimePlayed.Minutes + 1))
            .WithUpdatedDescription("Items Obtained Saved at: " + DateTime.Now);
        //Daha Sonra Ayarlanan Builder Objesi ile Build Edilerek SavedGameMetadataUpdate Objesi Olusturulur.
        SavedGameMetadataUpdate updatedMetadata = builder.Build();

        //Client Objesini Cagir.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        //Client Objesi Uzerinden Degisiklikleri Commit Et. CRUD Operasyonlari Ile Aynidir.
        savedGameClient.CommitUpdate(comingFromOpenGameMetaData, updatedMetadata, savedData, callback);
    }

    public void LoadGameData_From_GoogleCloudService(ISavedGameMetadata comingFromOpenGameMetaData)
    {
        //Verilen Metadataya Iliskin Kayitli Dosyayi Yukleme
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(comingFromOpenGameMetaData, (SavedGameRequestStatus status, byte[] data) =>
        {         
            if (status == SavedGameRequestStatus.Success)
            {
                //Serverdan gelen user datasini aktar. Byte to JSON Donusumu sagla.
                userSavedData = TranslateBytesToJSON(data);
                Debug.Log("Oyun Verileri Aktariliyor!"); //Logcat Test
            }
            else
            {
                //Eger basarisizsa zaten donen bir user data yoktur.
                Debug.Log("Oyun Verileri Aktarilamadi!"); //Logcat Test
            }
        });
    }

    #endregion Google Cloud Veri Yazma Metodlari

    #region Google Cloud Uzerinden Veri Okuma Metodlari

    private bool isBusy_ALoadGameData_Async = false;

    private string userSavedData;
    public IEnumerator ALoadGameData_Async(ISavedGameMetadata comingFromOpenGameMetaData)
    {
        //Mesgul ise IEnumerator metoddan cik.
        if(isBusy_ALoadGameData_Async == true)
        {
            yield break;
        }

        //IEnumerator mesgul degildi, bu kisma gecebildiyse artik mesgul true isaretle devam et.
        isBusy_ALoadGameData_Async = true;

        //Bu metod Google Cloud uzerinden kayitli veriyi getirir.
        bool response = false;

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(comingFromOpenGameMetaData, (SavedGameRequestStatus status, byte[] data) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                response = true;
                //Serverdan gelen user datasini aktar. Byte to JSON Donusumu sagla.
                userSavedData = TranslateBytesToJSON(data);
            }
            else
            {
                response = true;
                //Eger basarisizsa zaten donen bir user data yoktur.
            }
        });


        yield return new WaitUntil(() => response == true);

        //IEnumerator artik mesgul degil false isaretle.
        isBusy_ALoadGameData_Async = false;
    }

    #endregion Google Cloud Uzerinden Veri Okuma Metodlari

    #region Kaydedilen Datalari UI Ile Gosterme

    public void ShowSelectUI()
    {
        //Gosterilecek Saved Game Data Sayisi, Yeni Saved Game Datasi Olusturma Izni, Saved Game Datasi Silme Izni, UI Panel Adi, Callback Cagrilmasi.
        uint maxNumToDisplay = 5;
        bool allowCreateNew = false;
        bool allowDelete = true;
        string uiTitle = "Saved Games";

        //SavedGame Istemcisi Cagrilir.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ShowSelectSavedGameUI(uiTitle, maxNumToDisplay, allowCreateNew, allowDelete, OnSavedGameSelected);
       
    }

    public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata selectedGameMetaData)
    {
        //Status Degerlerine Gore Surecler Yonetilebilir.
        if (status == SelectUIStatus.SavedGameSelected)
        {
            log_Notification_Tmp.text = "Kaydedilen Kullanici Datalari Goruntuleme Basarili!";
            //Google Saved Game Panelinden Secilen Saved Game MetaDatasi Buraya Eklenir. Saved Game Yuklemesi Burada Gerceklisir.
            LoadGameData_From_GoogleCloudService(selectedGameMetaData);
        }
        else if (status == SelectUIStatus.AuthenticationError)
        {
            log_Notification_Tmp.text = "Saved Games Authentication Error!";
        }
        else if (status == SelectUIStatus.TimeoutError)
        {
            log_Notification_Tmp.text = "Saved Games TimeOut Error!";
        }
        else if (status == SelectUIStatus.UserClosedUI)
        {
            log_Notification_Tmp.text = "Saved Games Panel Closed!";
        }
        else
        {
            log_Notification_Tmp.text = "Saved Games Common Error!";
        }
    }

    #endregion Kaydedilen Datalari UI Ile Gosterme


    #region Kullanici Datalarini Google Play Servis Uzerinden Getirme Metodlari

    private List<ISavedGameMetadata> savedMetadataList = null;
    public IEnumerator AFetch_Saved_UserDatas_Async()
    {
        //Dosya Oncelikle Okuma ve Yazma Operasyonlari icin Acilmali Sorna Bu Metod Calistirilmalidir. 
        //DataSource > Cache&Network Daha Fazla Cakismaya Neden Olurken Internet Olmadigi Durumda Cache Bilgisini Alir. NetworkOnly Sadece Network Varke Calisir.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        bool response = false;
        bool isDataFetched = false;
        savedGameClient.FetchAllSavedGames(DataSource.ReadNetworkOnly, (status, metadataList) =>
        {
            if(status == SavedGameRequestStatus.Success)
            {
                response = true;
                isDataFetched = true;
                savedMetadataList = metadataList;
            }
            else
            {
                response = true;
                isDataFetched = false;
                savedMetadataList = null;
            }
        });

        yield return new WaitUntil(() => response == true);

        string str = "";

        if(isDataFetched == true)
        {
            str = "Metadata Dosyalari Basariyla Getirildi";
        }
        else
        {
           str = "Dosya Getirilemedi!";
        }

        log_Notification_Tmp.text = str;

    }

    public void Fetch_Saved_UserDatas_Async()
    {
        StartCoroutine(AFetch_Saved_UserDatas_Async());
    }

    public void List_All_The_Metadata_Files()
    {
        //Serverdan Getirilen Metadata Dosyasi Referans Degeri Null ise Metoda Devam Ettirilmez!
        if(savedMetadataList == null)
        {
            return;
        }

        string str = "";
        for(int i = 0; i < savedMetadataList.Count; i++)
        {
            str += i.ToString() + " " + savedMetadataList[i].Filename + "\n";
        }

        //TMP Referansi Null Degeri Almissa Burayi Yaptirma
        if(log_Notification_Tmp != null)
        {
            log_Notification_Tmp.text = str;
        }
    }

    #endregion Kullanici Datalarini Google Play Servis Uzerinden Getirme Metodlari

    public static List<string> FindIncrementalAchievements()
    {
        //Reflection Yontemiyle Static Alanlarin Listesi Elde Edilir.
        //GPS Static alanlarindaki GPS alanlarini bul ve liste olarak dondur.
        //GetValue null degerini aldiginda static alana erismek icin kullanildigini belirtir.
        return typeof(GPGSIds).GetFields().Select(x => x.GetValue(null).ToString()).ToList();
    }
}
