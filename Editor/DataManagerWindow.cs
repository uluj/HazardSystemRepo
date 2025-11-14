using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq; // TypeCache için System.Linq gerekebilir
using UnityEditor.TypeManagement; // TypeCache için bu namespace gerekli

public class DataManagerWindow : EditorWindow
{
    
    [MenuItem("Window/Teknik Tasarım/Veri Yöneticisi")]
    public static void ShowWindow()
    {
        GetWindow<DataManagerWindow>("Veri Yöneticisi");
    }

    private int selectedTab;
    private Vector2 scrollPosition;

    // "Tarifler" (C# Sınıfları)
    private List<Type> dataTypes = new List<Type>(); 
    // "Veri Varlıkları" (.asset dosyaları)
    private List<ScriptableObject> dataAssets = new List<ScriptableObject>();

    // Pencere açıldığında veya kod derlendiğinde çalışır
    private void OnEnable()
    {
        RefreshDataLists();
    }

    // Pencerenin arayüzünü (GUI) çizen ana fonksiyon
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Genel Veri Yöneticisi", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. Sekmeler (Toolbar)
        selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Veri Varlıkları (.asset)", "Veri Tarifleri (C# Sınıfları)" });
        EditorGUILayout.Space();

        // 2. Yenileme Butonu
        if (GUILayout.Button("Listeyi Yenile", GUILayout.Height(30)))
        {
            RefreshDataLists();
        }
        EditorGUILayout.Space();

        // 3. Kaydırılabilir Liste Alanı
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Hangi sekme seçiliyse o fonksiyonu çiz
        switch (selectedTab)
        {
            case 0:
                DrawAssetsTab();
                break;
            case 1:
                DrawTypesTab();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Projedeki tüm .asset dosyalarını listeleyen sekme.
    /// </summary>
    private void DrawAssetsTab()
    {
        if (dataAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("Projede 'ISystemData' arayüzünü kullanan hiçbir .asset dosyası bulunamadı.", MessageType.Info);
            return;
        }

        foreach (ScriptableObject asset in dataAssets)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // Asset'in kendisini gösteren alan (tıklanabilir)
            EditorGUILayout.ObjectField(asset.name, asset, asset.GetType(), false);
            
            // Proje penceresinde asset'i "ping"leyen (seçen) buton
            if (GUILayout.Button("Bul", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(asset);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Projedeki tüm C# "tarif" sınıflarını listeleyen ve
    /// yeni .asset yaratma butonu sağlayan sekme.
    /// </summary>
    private void DrawTypesTab()
    {
        if (dataTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("Projede 'ISystemData' arayüzünü ve 'ScriptableObject' sınıfını miras alan hiçbir C# script'i bulunamadı.", MessageType.Warning);
            EditorGUILayout.HelpBox("Örnek: public class ItemData : ScriptableObject, ISystemData { ... }", MessageType.Info);
            return;
        }

        foreach (Type type in dataTypes)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // Sınıfın adını (örn: "HazardData", "ItemData") göster
            EditorGUILayout.LabelField(type.Name);

            // '+' Butonu: Bu sınıftan (tariften) yeni bir .asset dosyası yaratır
            if (GUILayout.Button("+ Yeni Veri Yarat", GUILayout.Width(150)))
            {
                CreateNewAsset(type);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Bu, aracın motorudur. Projeyi tarar ve iki listeyi de doldurur.
    /// </summary>
    private void RefreshDataLists()
    {
        // 1. Tarifleri Bul (C# Sınıfları)
        dataTypes.Clear();
        // Unity'nin 'ISystemData' arayüzünü (interface) kullanan tüm tipleri (sınıfları)
        // bulmasını sağlayan güçlü bir fonksiyonu (Unity 2020+):
        var types = TypeCache.GetTypesImplementing<ISystemData>();

        foreach (var type in types)
        {
            // Eğer bu tip bir ScriptableObject ise VE abstract (soyut) değilse
            if (type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract)
            {
                dataTypes.Add(type);
            }
        }

        // 2. Varlıkları Bul (.asset dosyaları)
        dataAssets.Clear();
        // AssetDatabase'den projedeki TÜM ScriptableObject'lerin GUID'lerini (kimliklerini) bul
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            // Eğer bulunan ScriptableObject, bizim etiketimizi (ISystemData) taşıyorsa
            if (obj is ISystemData)
            {
                dataAssets.Add(obj);
            }
        }
        
        // Listeleri isme göre sırala
        dataAssets = dataAssets.OrderBy(a => a.name).ToList();
        dataTypes = dataTypes.OrderBy(t => t.Name).ToList();
        
        // Pencereye değişikliği fark etmesini söyle (Repaint)
        this.Repaint();
    }

    /// <summary>
    /// Seçilen 'type' (tarif) üzerinden yeni bir .asset dosyası oluşturur.
    /// </summary>
    private void CreateNewAsset(Type type)
    {
        // 1. Kullanıcıya "Nereye kaydedeyim?" diye soran bir panel aç
        string path = EditorUtility.SaveFilePanelInProject(
            "Yeni Veri Varlığı Yarat",             // Pencere Başlığı
            "New" + type.Name + ".asset",       // Varsayılan Dosya Adı
            "asset",                            // Dosya Uzantısı
            "Lütfen yeni veri dosyasının konumunu seçin."); // Mesaj

        // Eğer kullanıcı "İptal"e basarsa (path boş gelir)
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // 2. O tipten (sınıftan) boş bir kopya (instance) oluştur
        ScriptableObject newAsset = ScriptableObject.CreateInstance(type);

        // 3. Bu kopyayı, kullanıcının seçtiği yola bir .asset dosyası olarak kaydet
        AssetDatabase.CreateAsset(newAsset, path);
        AssetDatabase.SaveAssets(); // Veritabanını kaydet
        AssetDatabase.Refresh();    // Proje penceresini yenile

        // 4. Kullanıcıya kolaylık olsun diye yeni oluşturulan asset'i seç (ping)
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newAsset;
        EditorGUIUtility.PingObject(newAsset);

        // 5. Yeni asset'in "Veri Varlıkları" sekmesinde görünmesi için listeyi yenile
        RefreshDataLists();
    }
}