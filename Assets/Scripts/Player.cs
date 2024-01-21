using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] Animator _anim;
    [SerializeField] Rigidbody2D _rigidBody;
    [SerializeField] GroundCheck _headCheck;
    [SerializeField] GroundCheck _groundCheck;
    [SerializeField] float _jumpSpeed = 10f;
    [SerializeField] float _gravity = 20f;
    [SerializeField] float _jumpHeight = 3.0f;
    [SerializeField] float _jumpLimitTime = 1.0f;
    [SerializeField] AnimationCurve _dashCurve;
    [SerializeField] AnimationCurve _jumpCurve;
    public float speed = 5f;
    public float gameSpeed = 1.0f;
    public float MaxTime { get; set; } = 12.0f;
    public bool IsManualMode { get; set; } = false;
    public bool CanMove { get; set; } = false;
    public bool IsGoal { get; private set; } = false;
    public bool IsDead { get; private set; } = false;
    public int[] ActionIDs { get; set; } = new int[0];
    public UnityAction onEndMoveAction = null;
    bool _isGround = false;
    bool _isHead = false;
    bool _isJump = false;
    bool _isRun = false;
    float _jumpStartPosY = 0.0f;
    float _dashTime = 0.0f;
    float _jumpTime = 0.0f;
    float _beforeKey = 0.0f;
    readonly float minPosY = -12.0f;
    readonly string _enemyTag = "Enemy";
    readonly string _goalTag = "Goal";
    int _frameCount = 0;
    int _currentActionID = -1;
    int FramePerAction() => (int)(ActionIDs.Length / MaxTime) > 0 ? (int)(ActionIDs.Length / MaxTime) : 1;

    /// <summary> 
    /// Y成分で必要な計算をし、速度を返す。 
    /// </summary> 
    /// <returns>Y軸の速さ</returns> 
    private float GetYSpeed()
    {
        float verticalKey = Input.GetAxis("Vertical");
        float ySpeed = -_gravity;

        if (!IsManualMode)
        {
            verticalKey = _currentActionID switch
            {
                0 => 0.0f,
                1 => 1.0f,
                2 => 1.0f,
                _ => 0.0f,
            };
        }

        if (_isGround)
        {
            if (verticalKey > 0)
            {
                ySpeed = _jumpSpeed;
                _jumpStartPosY = transform.localPosition.y; //ジャンプした位置を記録する
                _isJump = true;
                _jumpTime = 0.0f;
            }
            else
            {
                _isJump = false;
            }
        }
        else if (_isJump)
        {
            //上方向キーを押しているか
            bool pushUpKey = verticalKey > 0;
            //現在の高さが飛べる高さより下か
            bool canHeight = _jumpStartPosY + _jumpHeight > transform.localPosition.y;
            //ジャンプ時間が長くなりすぎてないか
            bool canTime = _jumpLimitTime / gameSpeed > _jumpTime;

            if (pushUpKey && canHeight && canTime && !_isHead)
            {
                ySpeed = _jumpSpeed;
                _jumpTime += Time.deltaTime;
            }
            else
            {
                _isJump = false;
                _jumpTime = 0.0f;
            }
        }

        return ySpeed;
    }

    /// <summary> 
    /// X成分で必要な計算をし、速度を返す。 
    /// </summary> 
    /// <returns>X軸の速さ</returns> 
    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");
        float xSpeed = 0.0f;

        if (!IsManualMode)
        {
            horizontalKey = _currentActionID switch
            {
                0 => 1.0f,
                1 => 1.0f,
                2 => 0.0f,
                _ => 0.0f,
            };
        }

        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            _isRun = true;
            _dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            _isRun = true;
            _dashTime += Time.deltaTime;
            xSpeed = -speed;
        }
        else
        {
            _isRun = false;
            xSpeed = 0.0f;
            _dashTime = 0.0f;
        }

        //前回の入力からダッシュの反転を判断して速度を変える
        if (horizontalKey > 0 && _beforeKey < 0)
        {
            _dashTime = 0.0f;
        }
        else if (horizontalKey < 0 && _beforeKey > 0)
        {
            _dashTime = 0.0f;
        }

        _beforeKey = horizontalKey;
        _beforeKey = horizontalKey;
        return xSpeed;
    }

    /// <summary> 
    /// アニメーションを設定する 
    /// </summary> 
    private void SetAnimation()
    {
        _anim.SetBool("jump", _isJump);
        _anim.SetBool("ground", _isGround);
        _anim.SetBool("run", _isRun);
    }

    void EndPlay()
    {
        _rigidBody.Sleep();
        CanMove = false;
        IsDead = true;
    }

    void FixedUpdate()
    {
        if (!CanMove)
        {
            return;
        }

        if (transform.localPosition.y < minPosY)
        {
            EndPlay();
            return;
        }

        _frameCount++;

        int index = _frameCount / FramePerAction();

        if (!IsManualMode)
        {
            if (0 <= index && index <= ActionIDs.Length - 1)
            {
                _currentActionID = ActionIDs[index];
            }

            else
            {
                if (index > ActionIDs.Length - 1)
                {
                    EndPlay();
                    return;
                }

                _currentActionID = -1;
            }
        }

        if (!IsDead && !IsGoal && (IsManualMode || _currentActionID >= 0))
        {
            //接地判定を得る
            _isHead = _headCheck.IsGround;
            _isGround = _groundCheck.IsGround;

            //各種座標軸の速度を求める
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            //アニメーションを適用
            SetAnimation();

            // ジャンプ中は横方向減速
            // xSpeed *= _dashCurve.Evaluate(_dashTime) * (_isJump ? 0.5f : 1);
            xSpeed *= _isJump ? 0.5f : 1;

            // 同時押しの補正
            if (Mathf.Abs(xSpeed) > 0.0f && Mathf.Abs(ySpeed) > 0.0f)
            {
                xSpeed /= 1.4142f;
                ySpeed /= 1.4142f;
            }

            _rigidBody.velocity = new Vector2(xSpeed, ySpeed) * gameSpeed;
        }

        else
        {
            _rigidBody.velocity = new Vector2(0, -_gravity) * gameSpeed;
        }

        onEndMoveAction?.Invoke();
    }

    #region 接触判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead || IsGoal)
        {
            return;
        }

        if (collision.collider.tag == _enemyTag)
        {
            IsDead = true;

            _anim.Play("dead");

            EndPlay();
        }

        if (collision.collider.tag == _goalTag)
        {
            IsGoal = true;

            EndPlay();
        }
    }
    #endregion
}
