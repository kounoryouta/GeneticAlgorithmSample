using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    static string groundTag = "Ground";
    bool _isGround = false;
    bool _isGroundEnter = false, _isGroundStay = false, _isGroundExit = false;
    public bool IsGround
    {
        get
        {
            if (_isGroundEnter || _isGroundStay)
            {
                _isGround = true;
            }

            else if (_isGroundExit)
            {
                _isGround = false;
            }

            _isGroundEnter = false;
            _isGroundStay = false;
            _isGroundExit = false;

            return _isGround;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            _isGroundEnter = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            _isGroundStay = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            _isGroundExit = true;
        }
    }
}