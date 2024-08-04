using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class UnityStringEvent : UnityEvent<string>
{
    //Kullanici Tanimli String Parametre Alan UnityEvent
}

public class InputFieldManager : MonoBehaviour
{
    public Button userAproval_Btn;
    public TMP_InputField userCreatedData_Tmp_Inp;
    public TextMeshProUGUI userNotification_Tmp;

    //Buraya atanacak olan metodun dinamik varyanti secilmelidir. Note : Inspector Override Edilmesi Icindir!
    [Header("User Tarafindan Olusturulan Parametreli UnityEvent<TO> Kod Kullanimi Icin Dinamic Varyant Secilmelidir!")]
    public UnityStringEvent OnClick_PostData;
    
    private void Start()
    {
        //Referans Edilen Buttonun Listenerina Eklenir.
        userAproval_Btn.onClick.AddListener(PostUserData);
    }
    public void PostUserData()
    {
        //Bu kisim cok cok onemli! Metod UnityEvent uzerine inspector panel uzerinden atandiginda eger kodla override edilecekse dinamik metod seçenegi ile atanmalidir!
        //Dinamik olarak atanan metod varyasyonu invoke edileblir!
        string strDynamically = userNotification_Tmp.text = userCreatedData_Tmp_Inp.text;
        OnClick_PostData?.Invoke(strDynamically);
    }
}
