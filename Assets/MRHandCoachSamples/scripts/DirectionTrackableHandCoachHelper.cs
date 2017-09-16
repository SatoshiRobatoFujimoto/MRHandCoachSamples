using HUX.Interaction;
using HUX.Utility;
using UnityEngine;

[RequireComponent(typeof(HandCoach))]
public class DirectionTrackableHandCoachHelper : MonoBehaviour
{
    public HandCoach.HandGestureEnum TargetRightGesture;
    public HandCoach.HandDirectionEnum TargetRightDirection;

    public HandCoach.HandGestureEnum TargetLeftGesture;
    public HandCoach.HandDirectionEnum TargetLeftDirection;
    
    public float TapThreshold = 0.5f;

    [SerializeField]
    private LocalHandInput rHand;

    [SerializeField]
    private LocalHandInput lHand;


    private HandCoach _hand;

    private TrackingState rState ;
    private TrackingState lState ;


    // Use this for initialization
    private void Start()
    {
        _hand = gameObject.GetComponent<HandCoach>();
        rState = new TrackingState();
        lState = new TrackingState();
    }


    // Update is called once per frame
    private void Update()
    {

        Debug.Log(",rG:" + rState.CurrentGesture + ",rD:" + rState.CurrentDirection);
        Debug.Log(",lG" + lState.CurrentGesture + ",lD" + lState.CurrentDirection);
        var rHandState = InputSources.Instance.hands.GetHandState(rHand.Handedness, 0.1f);
        var lHandState = InputSources.Instance.hands.GetHandState(lHand.Handedness, 0.1f);
         rState = DetectTracking(rHand,rHandState,rState);
         lState = DetectTracking(lHand,lHandState,lState);

        _hand.Tracking = HandCoach.HandVisibilityEnum.None;
        if (rState.CurrentGesture == TargetRightGesture
            && (rState.CurrentDirection & TargetRightDirection )== TargetRightDirection)
        {
            _hand.Tracking = HandCoach.HandVisibilityEnum.Right;
        }
        if (lState.CurrentGesture == TargetLeftGesture
            && (lState.CurrentDirection & TargetLeftDirection )== TargetLeftDirection)
        {
           _hand.Tracking = _hand.Tracking | HandCoach.HandVisibilityEnum.Left;
        }
        

    }
    
    private TrackingState DetectTracking(LocalHandInput hand,InputSourceHands.CurrentHandState handState,TrackingState result)
    {

        if (handState == null)
        {
            result.CurrentGesture = HandCoach.HandGestureEnum.None;
        }
        else if (!result.PrevPressed && result.PrevTime < 0.001f && !handState.Pressed)
        {
            result.CurrentGesture = HandCoach.HandGestureEnum.Ready;
        }
        else if (result.PrevPressed && !handState.Pressed)
        {
            if (result.PrevTime < TapThreshold)
            {
                result.CurrentGesture = HandCoach.HandGestureEnum.Tap;
                result.PrevPressed = false;
                result.PrevTime = 0.0f;
            }
            else
            {
                result.CurrentGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                result.PrevPressed = false;
                result.PrevTime = 0.0f; 
            }
        }
        else if (handState.Pressed)
        {
            if (result.PrevTime > TapThreshold)
            {
                result.CurrentGesture = HandCoach.HandGestureEnum.TapHold;
            }
            result.PrevTime += Time.deltaTime;
            result.PrevPressed = true;
        }


        var all = HandCoach.HandDirectionEnum.Right
                  | HandCoach.HandDirectionEnum.Left
                  | HandCoach.HandDirectionEnum.Up
                  | HandCoach.HandDirectionEnum.Down
                  | HandCoach.HandDirectionEnum.Front
                  | HandCoach.HandDirectionEnum.Back;

        var localDeadZone = Vector3.zero;

        if (hand.LocalPosition.x > localDeadZone.x)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Right) &
                                    (all ^ HandCoach.HandDirectionEnum.Left);
        else if (hand.LocalPosition.x < -1f * localDeadZone.x)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Left) &
                                    (all ^ HandCoach.HandDirectionEnum.Right);
        else
            result.CurrentDirection = result.CurrentDirection &
                                    (all ^ (HandCoach.HandDirectionEnum.Left | HandCoach.HandDirectionEnum.Right));

        if (hand.LocalPosition.y > localDeadZone.y)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Up) &
                                    (all ^ HandCoach.HandDirectionEnum.Down);
        else if (hand.LocalPosition.y < -1f * localDeadZone.y)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Down) &
                                    (all ^ HandCoach.HandDirectionEnum.Up);
        else
            result.CurrentDirection = result.CurrentDirection &
                                    (all ^ (HandCoach.HandDirectionEnum.Up | HandCoach.HandDirectionEnum.Down));

        if (hand.LocalPosition.z > localDeadZone.z)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Front) &
                                    (all ^ HandCoach.HandDirectionEnum.Back);
        else if (hand.LocalPosition.z < -1f * localDeadZone.z)
            result.CurrentDirection = (result.CurrentDirection | HandCoach.HandDirectionEnum.Back) &
                                    (all ^ HandCoach.HandDirectionEnum.Front);
        else
            result.CurrentDirection = result.CurrentDirection &
                                    (all ^ (HandCoach.HandDirectionEnum.Back | HandCoach.HandDirectionEnum.Front));

        return result;
    }

    public struct TrackingState
    {
        private HandCoach.HandDirectionEnum currentDir;
        public HandCoach.HandGestureEnum CurrentGesture;

        public HandCoach.HandDirectionEnum CurrentDirection
        {
            get
            {
                if (CurrentGesture == HandCoach.HandGestureEnum.TapHold) return currentDir;
                return HandCoach.HandDirectionEnum.None;
            }
            set
            {
                currentDir = value;
                if (CurrentGesture != HandCoach.HandGestureEnum.TapHold) currentDir = HandCoach.HandDirectionEnum.None;
            }
        }

        public bool PrevPressed;
        public float PrevTime;

    }
}