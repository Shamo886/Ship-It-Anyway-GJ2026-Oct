using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class audioManger : MonoBehaviour
{
    [Header("把 AudioMixer 拖进来")]
    public AudioMixer m_audioMixer;

    [Header("把 UI Sliders 拖进来")]
    public Slider mainVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("设置游戏开始时滑块的初始位置（0到1）")]
    public float mainInitialValue = 1f;
    public float bgmInitialValue = 0.772f;
    public float sfxInitialValue = 1f;

    // 平滑过渡速度，数值越大跟手越快，越小越平滑（推荐 5~15）
    [Header("音量平滑速度")]
    public float smoothSpeed = 10f;

    // 内部记录当前平滑后的真实音量值（0~1范围）
    private float currentMainValue = 1f;
    private float currentBgmValue = 0.772f;
    private float currentSfxValue = 1f;

    void Start()
    {
        mainVolumeSlider.value = mainInitialValue;
        bgmVolumeSlider.value = bgmInitialValue;
        sfxVolumeSlider.value = sfxInitialValue;

        currentMainValue = mainInitialValue;
        currentBgmValue = bgmInitialValue;
        currentSfxValue = sfxInitialValue;

        ApplyAllVolumes(currentMainValue, currentBgmValue, currentSfxValue);
    }

    void Update()
    {
        // 用 Lerp 让当前值平滑逼近滑块的值
        // 这样快速拖拉时，音量是渐变过去的，不会产生突变爆音
        currentMainValue = Mathf.Lerp(currentMainValue, mainVolumeSlider.value, Time.unscaledDeltaTime * smoothSpeed);
        currentBgmValue = Mathf.Lerp(currentBgmValue, bgmVolumeSlider.value, Time.unscaledDeltaTime * smoothSpeed);
        currentSfxValue = Mathf.Lerp(currentSfxValue, sfxVolumeSlider.value, Time.unscaledDeltaTime * smoothSpeed);

        // 每帧把平滑后的值应用到 AudioMixer
        ApplyAllVolumes(currentMainValue, currentBgmValue, currentSfxValue);
    }

    // 统一应用三个音量的方法（内部做了 dB 转换和静音保护）
    private void ApplyAllVolumes(float mainVal, float bgmVal, float sfxVal)
    {
        m_audioMixer.SetFloat("主音量", ConvertToDecibel(mainVal));
        m_audioMixer.SetFloat("BGM", ConvertToDecibel(bgmVal));
        m_audioMixer.SetFloat("SFX", ConvertToDecibel(sfxVal));
    }

    // 把 0~1 的线性值转换为 dB 值
    private float ConvertToDecibel(float value)
    {
        if (value <= 0.0001f) return -80f; // 完全静音
        return Mathf.Log10(value) * 20f;
    }
}