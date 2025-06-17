using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;
public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer _leftlineRenderer;
    [SerializeField] private LineRenderer _rightlineRenderer;


    [Header("Transform References")]
    [SerializeField] private Transform _leftStartPosition;
    [SerializeField] private Transform _rightStartPosition;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _idlePosition;
    [SerializeField] private Transform _elasticTransform;

    [Header("Slingshot Stats")]
    [SerializeField] private float maxDistance = 3.5f;
    [SerializeField] private float _shotForce = 3.5f;
    [SerializeField] private float _timebnBirdResponse = 2f;
    [SerializeField] private float _elasticDivider = 1.2f;

    [Header("Scripts")]
    [SerializeField] private SlingShotArea _slingShotArea;
    // [SerializeField] private CameraManager _cameraManager;

    [Header("Bird")]
    [SerializeField] private AngieBird _angieBirdPrefab;
    [SerializeField] private float _angieBirdPositionOffset;

    private Vector2 _slingShotLinesPosition;
    private Vector2 _direction;
    private Vector2 _directionNormalized;
    private bool _clickedWithinArea;
    private bool _birdOnSlingShot;
    private AngieBird _spawnedAngieBird;

    //// Called when the object is initialized, disables line renderers and spawns the first bird
    private void Awake()
    {
        _leftlineRenderer.enabled = false;
        _rightlineRenderer.enabled = false;
        SpawnAngieBird();
    }

    //// Called every frame to check for mouse input and handle dragging, launching, and resetting the bird
    private void Update()
    {

        if (Mouse.current.leftButton.isPressed)
        {
            Debug.Log("clicked");
        }
        Debug.Log("Update is running");
        if (InputManager.WasLeftMouseButtonPressed && _slingShotArea.IsWithinSlingShotArea())

        {
            _clickedWithinArea = true;
            Debug.Log("Mouse clicked");
        }

        if (InputManager.IsLeftMousePressed && _clickedWithinArea && _birdOnSlingShot)

        {
            DrawSlingShot();
            PositionAndRotateAngieBird();
            Debug.Log("Mouse is in slingshot area");
        }

        if (InputManager.WasLeftMouseButtonReleased && _birdOnSlingShot)

        {
            Debug.Log("Mouse released, launching bird");
            if (GameManager.instance.HasEnoughShots())
            {
                _clickedWithinArea = false;
                float stretchForce = _direction.magnitude * _shotForce;

                _spawnedAngieBird.LaunchBird(_direction, stretchForce);
                GameManager.instance.UseShot();
                _birdOnSlingShot = false;
                SetLines(_centerPosition.position);
                AnimateSlingShot();

                if (GameManager.instance.HasEnoughShots())
                {
                    StartCoroutine(SpawnAngieBirdAfterTime());
                }

            }


        }
    }

    #region SlingShotMethods

    //// Calculates the position of the pulled slingshot band based on mouse position, clamps to max distance
    private void DrawSlingShot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(InputManager.MousePosition);
        Debug.Log($"Touch Position (World): {touchPosition}");
        touchPosition.z = 0f;

        Vector3 offset = touchPosition - _centerPosition.position;
        Vector3 clampedOffset = Vector3.ClampMagnitude(offset, maxDistance);
        Vector3 finalPosition = _centerPosition.position + clampedOffset;

        _slingShotLinesPosition = finalPosition;
        SetLines(finalPosition);

        _direction = (Vector2)_centerPosition.position - _slingShotLinesPosition;
        _directionNormalized = _direction.normalized;
    }

    // Updates the LineRenderers to visually represent the stretched slingshot bands
    private void SetLines(Vector2 position)
    {
        if (!_leftlineRenderer.enabled && !_rightlineRenderer.enabled)
        {
            _leftlineRenderer.enabled = true;
            _rightlineRenderer.enabled = true;
        }

        _leftlineRenderer.SetPosition(0, position);
        _leftlineRenderer.SetPosition(1, _leftStartPosition.position);

        _rightlineRenderer.SetPosition(0, position);
        _rightlineRenderer.SetPosition(1, _rightStartPosition.position);
    }

    #endregion

    #region AngieBirdMethods


    // Spawns a new AngieBird in the idle position and aims it toward the center of the slingshot
    private void SpawnAngieBird()
    {
        SetLines(_idlePosition.position);
        Vector2 dir = ((Vector2)_centerPosition.position - (Vector2)_idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)_idlePosition.position + dir * _angieBirdPositionOffset;

        _spawnedAngieBird = Instantiate(_angieBirdPrefab, spawnPosition, Quaternion.identity);
        _spawnedAngieBird.transform.right = dir;
        _birdOnSlingShot = true;
    }

    // Updates the position and rotation of the bird while dragging to match slingshot direction
    private void PositionAndRotateAngieBird()
    {
        if (_spawnedAngieBird == null) return;

        _spawnedAngieBird.transform.position = _slingShotLinesPosition + _directionNormalized * _angieBirdPositionOffset;
        _spawnedAngieBird.transform.right = _directionNormalized;
    }

    // Waits a short time before spawning the next bird (to give feedback and delay between shots)
    private IEnumerator SpawnAngieBirdAfterTime()
    {
        yield return new WaitForSeconds(_timebnBirdResponse);
        SpawnAngieBird();
        // yield return null;
    }
    #endregion

    #region  Animate Slingshot


    // Animates the slingshot band snapping back after releasing a shot using tweening
    private void AnimateSlingShot()
    {
        _elasticTransform.position = _leftlineRenderer.GetPosition(0);
        float dist = Vector2.Distance(_elasticTransform.position, _centerPosition.position);
        float time = dist / _elasticDivider;

        _elasticTransform.DOMove(_centerPosition.position, time).SetEase(Ease.OutElastic);
        StartCoroutine(AnimateSlingShotLines(_elasticTransform, time));
    }

    // Coroutine that updates the slingshot band position during the snap-back animation
    private IEnumerator AnimateSlingShotLines(Transform trans, float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            SetLines(trans.position);
            yield return null;
        }
    }
    #endregion
}
