using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JengaPiece : MonoBehaviour
{
    #region Fields
    [Header("Jenga Materials")]
    [SerializeField]
    private Material _glassMaterial;
    [SerializeField]
    private Material _woodMaterial;
    [SerializeField]
    private Material _stoneMaterial;
    [SerializeField]
    private string _glassText;
    [SerializeField]
    private string _woodText;
    [SerializeField]
    private string _stoneText;
    [SerializeField]
    private TextMesh _text1, _text2;

    [SerializeField]
    private JengaType _type;

    private JengaType _previousType;

    private DataObject _dataObject;

    private MeshRenderer _meshRenderer;
    private Rigidbody _rigidbody;

    public enum JengaType
    {
        Glass,
        Wood,
        Stone
    };

    private Dictionary<int, JengaType> masteryDictionary = new Dictionary<int, JengaType>() {
        {0, JengaType.Glass },
        {1, JengaType.Wood },
        {2, JengaType.Stone }
    };
    #endregion

    private void Awake() {
        OnJengaTypeChanged(_type);
        _meshRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetData(DataObject dataObject) {
        _dataObject = dataObject;
        OnJengaTypeChanged(dataObject.mastery);
    }

    public void ToggleGravity(bool value) {
        _rigidbody.useGravity = value;
        _rigidbody.isKinematic = !value;
    }

    public void OnJengaTypeChanged(int masteryValue) {
        if (masteryDictionary.TryGetValue(masteryValue, out JengaType value)) {
            OnJengaTypeChanged(masteryDictionary[masteryValue]);
        } else {
            Debug.LogError("Unsupported mastery value detected! " + masteryValue);
        }
    }

    public void OnJengaTypeChanged(JengaType jengaType) {
        Material newMaterial = null;
        string shownText = null;
        switch (jengaType) {
            case JengaType.Glass:
                newMaterial = _glassMaterial;
                shownText = _glassText;
                break;
            case JengaType.Wood:
                newMaterial = _woodMaterial;
                shownText = _woodText;
                break;
            case JengaType.Stone:
                newMaterial = _stoneMaterial;
                shownText = _stoneText;
                break;
            default:
                break;
        }

        if (newMaterial != null && _meshRenderer != null) {
            _meshRenderer.sharedMaterial = newMaterial;
        }

        if (shownText != null) {
            _text1.text = shownText;
            _text2.text = shownText;
        }
    }

    private void OnValidate() {
        if (_previousType != _type) {
            OnJengaTypeChanged(_type);
            _previousType = _type;
        }
    }

    public override string ToString() {
        string infoString = "";

        infoString += $"{_dataObject.grade}:{ _dataObject.domain}\n";
        infoString += $"{_dataObject.cluster}";
        infoString += $"{_dataObject.standardid}:{ _dataObject.standarddescription}\n";

        return infoString;
    }

}
