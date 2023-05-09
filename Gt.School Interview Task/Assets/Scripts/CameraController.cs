using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private float rotateSpeed = 5;
    [SerializeField]
    private float zoomSpeed = 2;
    [SerializeField]
    private float minZoomDistance = 1;
    [SerializeField]
    private float maxZoomDistance = 10;
    [SerializeField]
    private TMP_Text _infoText;

    private Transform _target;
    private float currentZoomDistance = 5;
    private string jengaPieceTag = "JengaPiece";
    private JengaPiece _detailsForJengaPiece;
    #endregion

    private void Awake() {
        _infoText.text = "";
    }

    private void Update() {
        // Rotate camera around target while left mouse button is pressed
        if (_target != null && Input.GetMouseButton(0)) {
            HandleLeftMouseClick();
        }

        HandleMouseScroll();

        // Change target while middle mouse button is pressed
        if (Input.GetMouseButton(2)) {
            HandleMiddleMouseClick();
        }

        if (Input.GetMouseButtonDown(1)) {
            HandleRightMouseClick();
        }

        // Position camera based on current zoom distance
        if (_target != null)
            transform.position = _target.position - transform.forward * currentZoomDistance;
    }

    private void HandleLeftMouseClick() {
        float x = Input.GetAxis("Mouse X") * rotateSpeed;
        float y = Input.GetAxis("Mouse Y") * rotateSpeed;

        transform.RotateAround(_target.position, Vector3.up, x);
        transform.RotateAround(_target.position, transform.right, -y);
    }

    private void HandleMiddleMouseClick() {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            if (hit.transform.gameObject.tag == jengaPieceTag) {
                _target = hit.transform;
                transform.LookAt(_target);
            }
        }
    }

    private void HandleRightMouseClick() {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            if (hit.transform.gameObject.tag == jengaPieceTag) {
                _detailsForJengaPiece = hit.transform.GetComponent<JengaPiece>();
                _infoText.text = _detailsForJengaPiece.ToString();
            }
        }
    }

    private void HandleMouseScroll() {
        // Zoom in and out using mouse wheel
        float zoomAmount = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoomDistance -= zoomAmount;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
    }
}
