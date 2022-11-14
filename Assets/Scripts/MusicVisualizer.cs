using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MusicVisualizer : MonoBehaviour
{
    public AudioClip clip;

    public string lrcUrl;

    public FFTWindow windowType = FFTWindow.Rectangular;

    public Transform go;

    public float minHeight = 5;
    public float maxHeight = 540;
    public int dataLen = 64;
    public float lerpTime = 1;
    public float rotateSpeed;

    private AudioSource _audioSource;
    private Transform[] _cubes;
    private MeshRenderer[] _mats;
    private MaterialPropertyBlock _propertyBlock;

    private Lrc _lrc;
    private float[] _samples;
    private float[] _sampleBuffers;
    private float[] _bufferDecrease;
    private static readonly int Intensity = Shader.PropertyToID("_Glow");


    // Start is called before the first frame update
    void Start()
    {
        _audioSource = new GameObject("Audio Source").AddComponent<AudioSource>();

        _propertyBlock = new MaterialPropertyBlock();


        InitShapes();

        _samples = new float[1024];
        _sampleBuffers = new float[1024];
        _bufferDecrease = new float[1024];

        if (!clip)
        {
            return;
        }

        _lrc = Lrc.InitLrc(Application.dataPath + "/" + lrcUrl);


        _audioSource.clip = clip;
        _audioSource.Play();
    }

    private void InitShapes()
    {
        _cubes = new Transform[dataLen];
        _mats = new MeshRenderer[dataLen];


        for (int i = 0; i < dataLen / 2; i++)
        {
            CreateCube(i);
            CreateCube(dataLen / 2 + i);
        }
    }

    private void CreateCube(int index)
    {
        float angle = 2 * Mathf.PI / dataLen;

        Transform cube = Instantiate(go, transform).transform;

        cube.gameObject.SetActive(true);

        float curAngle = angle * index;

        cube.position = new Vector3(dataLen / 4 * Mathf.Cos(curAngle), 0, dataLen / 4 * Mathf.Sin(curAngle));

        _cubes[index] = cube;

        _mats[index] = cube.GetComponent<MeshRenderer>();

        _propertyBlock.SetFloat(Intensity, 0.05f);
        _mats[index].SetPropertyBlock(_propertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_audioSource.isPlaying)
        {
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _audioSource.time+=Time.deltaTime;
        }
        
        transform.Rotate(Vector3.down * rotateSpeed, Space.World);

        _audioSource.GetSpectrumData(_samples, 0, windowType);

        DecreaseSamples();

        for (int i = 0; i < _cubes.Length; i++)
        {
            ExecuteIndexCube(i);
        }
    }

    private void ExecuteIndexCube(int i)
    {
        float value = _sampleBuffers[i];

        float oldScale = _cubes[i].localScale.y;

        float newScale = minHeight + (maxHeight - minHeight) * value;

        _cubes[i].localScale = new Vector3(1, Mathf.Lerp(oldScale, newScale, lerpTime), 1);

        _propertyBlock.SetFloat(Intensity, Mathf.Clamp(0.5f + 8f * value, 0.5f, 1.5f));
        _mats[i].SetPropertyBlock(_propertyBlock);
    }

    private void DecreaseSamples()
    {
        for (int i = 0; i < dataLen; i++)
        {
            if (_samples[i] > _sampleBuffers[i])
            {
                _sampleBuffers[i] = _samples[i];
                _bufferDecrease[i] = 0.005f;
            }

            if (_samples[i] < _sampleBuffers[i])
            {
                _sampleBuffers[i] -= _bufferDecrease[i];
                _bufferDecrease[i] *= 1.2f;
            }
        }
    }
}