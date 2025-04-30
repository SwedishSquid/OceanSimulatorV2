using UnityEngine;

public class ShipMover : MonoBehaviour
{
    [HideInInspector]
    public Vector3 MovementDirection { get; private set; }

    public float Speed { get; private set; }

    public Oscillator RollRotationOscillator { get; private set; }  // rotation about front-back axis

    public Oscillator PitchRotationOscillator { get; private set; } // rotation about side-to-side axis

    public Oscillator HeaveMotionOscillator { get; private set; }   // motion along up-down axis

    public Vector3 InitialPosition { get; private set; }

    public Quaternion ReferenceRotation { get; private set; }

    private void Start()
    {
        InitialPosition = transform.position;
    }

    public void SetParamsAndReset(Vector3 movementDirection, float speed,
        Oscillator rollOscillator, Oscillator pitchOscillator, Oscillator heaveOscillator)
    {
        transform.rotation = Quaternion.identity;
        transform.position = InitialPosition;
        MovementDirection = movementDirection;
        transform.right = -movementDirection;
        ReferenceRotation = transform.rotation;
        Speed = speed;
        RollRotationOscillator = rollOscillator;
        PitchRotationOscillator = pitchOscillator;
        HeaveMotionOscillator = heaveOscillator;
        AdjustPosition(0);
    }

    public void AdjustPosition(float elapsedEpisodeTime)
    {
        var heave = HeaveMotionOscillator.Sample(elapsedEpisodeTime);
        transform.position = elapsedEpisodeTime * Speed * MovementDirection + InitialPosition + Vector3.up * heave;
        var roll = RollRotationOscillator.Sample(elapsedEpisodeTime);
        var pitch = PitchRotationOscillator.Sample(elapsedEpisodeTime);
        transform.rotation = ReferenceRotation * Quaternion.Euler(roll, 0, pitch);
    }
}
