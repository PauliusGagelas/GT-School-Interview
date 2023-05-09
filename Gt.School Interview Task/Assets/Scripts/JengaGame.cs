using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JengaGame : MonoBehaviour
{
    #region Fields
    [Header("Positional Customization")]
    [SerializeField]
    private Vector3 _stackInitialPosition = new Vector3(-4, 0, 0);
    [SerializeField]
    private Vector3 _stackOffset = new Vector3(2, 0, 0);
    [SerializeField]
    private Vector3 _stackNameOffset = new Vector3(0.25f, 0, -0.8f);
    [SerializeField]
    private Vector3 _stackNameScale = new Vector3(0.075f, 0.075f, 0.075f);
    [SerializeField]
    private float _horizontalOffset = 0.25f;
    [SerializeField]
    private float _verticalOffset = 0.15f;

    [Space]
    [Header("Visual Customization")]
    [SerializeField]
    private int _amountPerRow = 3;
    [SerializeField]
    private GameObject _jengaBlockPrefab;
    [SerializeField]
    private TextAsset _apiData;
    [SerializeField]
    private bool _useOnlineData;

    [Space]
    [Header("Scene Buttons")]
    [SerializeField]
    private Button _toggleGravityButton;
    [SerializeField]
    private Button _testMyStackButton;
    [SerializeField]
    private Button _resetButton;

    private bool _gravityOn = false;
    private string _apiDataURL = @"https://ga1vqcu3o1.execute-api.us-east-1.amazonaws.com/Assessment/stack";
    private RootObject _rootObject;
    #endregion


    private void Awake() {
        ProcessData();
        AddListeners();
    }

    private void OnDestroy() {
        RemoveListeners();
    }

    #region Button Logic

    private void AddListeners() {
        _toggleGravityButton.onClick.AddListener(OnGravityPressed);
        _testMyStackButton.onClick.AddListener(OnTestMyStackPressed);
        _resetButton.onClick.AddListener(OnResetPressed);
    }

    private void RemoveListeners() {
        _toggleGravityButton.onClick.RemoveListener(OnGravityPressed);
        _testMyStackButton.onClick.RemoveListener(OnTestMyStackPressed);
        _resetButton.onClick.RemoveListener(OnResetPressed);
    }

    private void OnTestMyStackPressed() {
        if (_rootObject == null)
            return;

        _gravityOn = false;
        OnGravityPressed();

        foreach (DataObject dataObject in _rootObject.dataObjects.ToArray()) {
            if (dataObject.mastery == 0) {
                Destroy(dataObject.objectReference.gameObject);
                _rootObject.dataObjects.Remove(dataObject);
            }
        }
    }

    private void OnGravityPressed() {
        if (_rootObject == null)
            return;

        _gravityOn = !_gravityOn;

        foreach (DataObject dataObject in _rootObject.dataObjects) {
            dataObject.objectReference.ToggleGravity(_gravityOn);
        }
    }

    private void OnResetPressed() {
        if (_rootObject == null)
            return;

        DestroyAllChildren();
        _gravityOn = false;
        ProcessData();
    }
    #endregion

    private void DestroyAllChildren() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void ProcessData() {
        if (_useOnlineData) {
            StartCoroutine(GetRequest(_apiDataURL, ParseData));
        } else if (_apiData != null) {
            ParseData(_apiData.text);
        } else {
            Debug.LogError("Could not process data properly!");
        }
    }

    private IEnumerator GetRequest(string uri, Action<string> callback) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string data = webRequest.downloadHandler.text;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    callback.Invoke(data);
                    break;
            }
        }
    }

    private void ParseData(string data) {
        try {
            RootObject rootObject = JsonUtility.FromJson<RootObject>("{\"dataObjects\":" + data + "}");
            AssignData(rootObject);
        } catch (Exception e) {
            Debug.LogError("Could not parse data: " + e);
        }
    }

    private void AssignData(RootObject rootObject) {
        // Extract stacks per grade
        Dictionary<string, List<DataObject>> extractedGrades = ExtractProperty("grade", rootObject);

        List<DataObject> orderedDataObjects;

        // For each stack order it correctly
        foreach (string grade in extractedGrades.Keys.ToList()) {           
            orderedDataObjects = extractedGrades[grade].OrderBy(x => x.domain).ThenBy(x => x.cluster).ThenBy(x => x.standardid).ToList();
            extractedGrades[grade] = orderedDataObjects;
        }

        _rootObject = rootObject;

        int stackIndex = 0;
        foreach (string grade in extractedGrades.Keys) {
            RenderGrade(extractedGrades[grade], grade, stackIndex);
            stackIndex++;
        }
    }

    private void RenderGrade(List<DataObject> dataObjects, string grade, int stackIndex) {
        int rowIndex = 0;
        int heightIndex = 0;
        // Render the stack            
        int currentIndex = 0;

        bool firstRowPassed = false;
        GameObject rowParent = null;

        // Make parents for each key
        GameObject stackParent = new GameObject();
        stackParent.transform.parent = gameObject.transform;
        stackParent.transform.name = grade;
        Vector3 offSet = stackIndex * _stackOffset + _stackInitialPosition; // Add initial stack offset
        Vector3 currentMiddlePosition = offSet;
        stackParent.transform.position = stackIndex * _stackOffset + _stackInitialPosition;

        // Make world Space Text field
        MakeGradeTitle(grade, stackParent.transform);
        
        for (int i = 0; i < dataObjects.Count; i++) {
            offSet = stackIndex * _stackOffset + _stackInitialPosition; // Add initial stack offset
                                                                        // Time to increase 
            if (!firstRowPassed || currentIndex >= _amountPerRow) {
                #region Make new Row Parent
                if (rowParent != null && heightIndex % 2 == 0) {
                    rowParent.transform.localEulerAngles = new Vector3(rowParent.transform.localEulerAngles.x, rowParent.transform.localEulerAngles.y + 90, rowParent.transform.localEulerAngles.z);
                }
                rowParent = new GameObject();
                rowParent.transform.parent = stackParent.transform;
                rowParent.transform.name = "Height: " + heightIndex;
                currentMiddlePosition = offSet;
                currentMiddlePosition.x += _horizontalOffset;
                currentMiddlePosition.y += _verticalOffset * heightIndex;
                rowParent.transform.position = currentMiddlePosition;
                #endregion

                currentIndex = 0;
                rowIndex = 0;

                if (firstRowPassed)
                    heightIndex++;
                firstRowPassed = true;
            }

            DataObject dataObject = dataObjects[i];
            RenderJengaBlock(offSet, rowIndex, heightIndex, rowParent.transform, ref dataObject);
            dataObjects[i] = dataObject;

            rowIndex++;
            currentIndex++;
        }
    }

    private void RenderJengaBlock(Vector3 offSet, int rowIndex, int heightIndex, Transform parentTransform, ref DataObject dataObject) {
        offSet.x += rowIndex * _horizontalOffset;
        offSet.y += heightIndex * _verticalOffset;
        GameObject jengaBlock = Instantiate(_jengaBlockPrefab);
        jengaBlock.transform.position = offSet;
        jengaBlock.transform.parent = parentTransform.transform;

        // Assign the correct type of jenga block
        JengaPiece jengaPiece = jengaBlock.GetComponentInChildren<JengaPiece>();
        DataObject data = dataObject;
        jengaPiece.SetData(data);
        dataObject.objectReference = jengaPiece;
    }

    private void MakeGradeTitle(string title, Transform parent) {
        GameObject stackTextParent = new GameObject();
        stackTextParent.transform.parent = parent.transform;
        stackTextParent.transform.name = title + " Text";
        stackTextParent.transform.localPosition = _stackNameOffset;
        stackTextParent.transform.localScale = _stackNameScale;
        TextMesh textMesh = stackTextParent.AddComponent<TextMesh>();
        textMesh.text = title;
        textMesh.anchor = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// Method used for extracting all the Grades
    /// </summary>
    /// <param name="keyToExtract"></param>
    /// <param name="rootObject"></param>
    /// <returns></returns>
    private Dictionary<string, List<DataObject>> ExtractProperty(string keyToExtract, RootObject rootObject) {
        Dictionary<string, List<DataObject>> extractedProperties = new Dictionary<string, List<DataObject>>();

        foreach (DataObject dataObject in rootObject.dataObjects) {
            string value = dataObject.grade;

            if (extractedProperties.ContainsKey(value)) {
                extractedProperties[value].Add(dataObject);
            } else {
                extractedProperties.Add(value, new List<DataObject>() { dataObject });
            }
        }

        return extractedProperties;
    }
}
