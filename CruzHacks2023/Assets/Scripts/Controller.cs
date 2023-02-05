using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public float scrollSpeed;
    public GameObject viewer;

    private GameObject selectionObjPreviouslySelected;
    public UIManager manager;

    public MatrixBlender mBlender;

    public StarSystem selectedSystem;

    private void Update() {
        #region WASD
        if (!manager.hasUIElementsOpen && !mBlender.transitionCoroutineIsActive) {
            if (Input.GetKey(KeyCode.W)) {
                viewer.transform.position += new Vector3(0f, 0f, scrollSpeed) * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A)) {
                viewer.transform.position += new Vector3(-scrollSpeed, 0f, 0f) * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.S)) {
                viewer.transform.position += new Vector3(0f, 0f, -scrollSpeed) * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D)) {
                viewer.transform.position += new Vector3(scrollSpeed, 0f, 0f) * Time.deltaTime;
            }

        }
        #endregion

        if (!manager.hasUIElementsOpen && !mBlender.transitionCoroutineIsActive) {
            OnLeftClick();
        }

        if (!manager.hasUIElementsOpen && !mBlender.transitionCoroutineIsActive) {
            //Enable and disable selection outlines around stars on hover
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100)) {
                StarSystem s = hit.transform.GetComponent<StarSystem>();
                s.selectionObj.SetActive(true);
                selectionObjPreviouslySelected = s.selectionObj;
            }
            else {
                if (selectionObjPreviouslySelected != null)
                    selectionObjPreviouslySelected.SetActive(false);
            }
        }

    }

    private void OnLeftClick() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100)) {
                selectedSystem = hit.transform.GetComponent<StarSystem>();
                //Debug.Log(hit.transform.GetComponent<StarSystem>().SystemProperties());
                manager.EnableStarInfo(selectedSystem); // selectedSystem.star.sClass, selectedSystem.star.size);
                manager.hasUIElementsOpen = true;
                if (selectionObjPreviouslySelected != null)
                    selectionObjPreviouslySelected.SetActive(false);
                Camera.main.fieldOfView = 34;
                //Camera.main.orthographic = false;

                //perspective/orthographic lerp code adapted from https://forum.unity.com/threads/smooth-transition-between-perspective-and-orthographic-modes.32765/
                float aspect = (float)Screen.width / (float)Screen.height;
                float fov = 60f;
                float near = 0.3f;
                float far = 1000f;
                Matrix4x4 perspective = Matrix4x4.Perspective(fov, aspect, near, far);
                mBlender.BlendToMatrix(perspective, 1f);


                //manager.originalPosWhenUIClicked = viewer.transform.position;
                StartCoroutine(manager.ZoomToLocation(viewer, new Vector3(hit.transform.position.x - 0.7f, 5, hit.transform.position.z)));
            }
        }
    }


}
