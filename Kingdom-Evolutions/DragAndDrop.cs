using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DragAndDrop : MonoBehaviour
{
    public float smoothTime = 0.3f;
    public Vector2 offset = new Vector2(0f, 0.5f);
    
    Person person;
    PersonController personController;
    Camera cam;
    
    public float timeBetweenClicks = 0.1f;
    float clickTimer = 0;
    float lastClickTime = 0;

    MouseState mouseState = MouseState.RELEASED;

    // Start is called before the first frame update
    void Start()
    {
        person = GetComponent<Person>();
        personController = GetComponent<PersonController>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetGameState() == GameState.MENU || GameManager.instance.GetFrame() != personController.frameParent) return;

        if (Input.GetMouseButtonDown(0))
        {
            CheckForDoubleClick();

            CheckIfClickedOnMe();

            lastClickTime = Time.time;
        }

        if (mouseState == MouseState.HOLDING)
        {
            OnMouseHold();
        }

        if (personController.GetPersonState() == PersonState.DRAGGING)
        {
            Drag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseRelease();
        }
    }

    void Drag()
    {
        Vector2 touchPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector3.Lerp(transform.position, touchPosition + offset, smoothTime);

        transform.position = new Vector2(Mathf.Clamp(transform.position.x, personController.GetXLimit().x, personController.GetXLimit().y),
                                        (Mathf.Clamp(transform.position.y, personController.GetYLimit().x, personController.GetYLimit().y)));
        
    }

    bool CheckIfClickedOnMe()
    {
        Vector2 touchPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Person closestPersonToClick = GameManager.instance.FindClosestPerson(touchPosition, 0.5f);

        if (person == closestPersonToClick)
        {
            mouseState = MouseState.HOLDING;
            return true;
        }

        return false;
    }

    void OnMouseRelease()
    {
        if (personController.GetPersonState() == PersonState.DRAGGING)
        {
            Drop();
        } 
        else if(clickTimer <= 0.15f)
        {
            if (CheckIfClickedOnMe())
            {
                Select();
            }
        }

        mouseState = MouseState.RELEASED;
        clickTimer = 0;
    }

    void OnMouseHold()
    {
        clickTimer += Time.deltaTime;

        if (clickTimer > 0.12f)
        {
            Grab();
        }
    }

    void CheckForDoubleClick()
    {
        if (Time.time - lastClickTime <= timeBetweenClicks)
        {
            person.DoubleClick();
        }
    }
    
    void Grab()
    {
        if (GameManager.instance.GetInteractState() != InteractState.HOLDING_UNIT)
        {

            personController.ChangePersonState(PersonState.DRAGGING);
            GameManager.instance.ChangeInteractState(InteractState.HOLDING_UNIT);

            //Animation
            SoundManager.instance.Play("Pickup");
            personController.BringInFront();

        }
    }

    void Select()
    {
        if (GameManager.instance.GetInteractState() != InteractState.HOLDING_UNIT)
        {
            GameManager.instance.SelectPerson(person);
            GameManager.instance.ChangeInteractState(InteractState.UNIT_SELECTED);
        }
    }

    void Drop()
    {
        personController.ChangePersonState(PersonState.MOVING);
        GameManager.instance.ChangeInteractState(InteractState.FREE);

        SoundManager.instance.Play("Drop");
        transform.DOScale(1f, 0.15f).OnComplete(() => transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 0));

        Vector2 touchPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Person closest = GameManager.instance.FindClosestPerson(person);

        if(closest != null)
        {
            person.CombinePeople(closest);
        }
    }
}

enum MouseState
{
    HOLDING,
    RELEASED
}

