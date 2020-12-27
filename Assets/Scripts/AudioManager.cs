/*
 *
 *  开发时间：2018.11.20
 *
 *  功能：用来对项目中的所有音频做同一的管理
 *
 *  描述：
 *      1、挂载该脚本的游戏物体上要挂载三个AudioSouce[可以修改脚本，动态挂]
 *      2、建议大的背景音乐不要加入AudioClip[]，对内存消耗大。而是哪里用到，单独使用PlayBackground函数播放
 *      3、小的音效片段可以将其加入AudioClip[]中，可以很方便的管理，可以通过声音剪辑、声音剪辑名称来进行音乐的播放
 *      4、该声音管理可以进行背景音乐的播放、音效的播放、背景音乐的音调改变、音效的音调改变、停止播放
 *
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

    public class AudioManager : MonoBehaviour
    {
        public AudioClip[] AudioClipArray;                               //剪辑数组

        public static float AudioBackgroundVolumns = 1F;                 //背景音量
        public static float AudioEffectVolumns = 1F;                     //音效音量
        public static float AudioBackgroundPitch = 1f;                   //背景音乐的音调
        public static float AudioEffectPitch = 1.0f;                     //音效的音调


        private static Dictionary<string, AudioClip> _DicAudioClipLib;   //音频库，将声音名字和声音资源进行关联

        private static AudioSource[] _AudioSourceArray;                  //音频源数组

        private static AudioSource _AudioSource_BackgroundAudio;         //背景音乐
        private static AudioSource _AudioSource_AudioEffectA;            //音效源A
        private static AudioSource _AudioSource_AudioEffectB;            //音效源B
        //......可以按需求进行添加

        /// <summary>
        /// 音效库资源加载
        /// </summary>
        void Awake()
        {
            //音频库加载[初始化，将音乐剪辑和名字联系起来]
            _DicAudioClipLib = new Dictionary<string, AudioClip>();

            foreach (AudioClip audioClip in AudioClipArray)
            {
                _DicAudioClipLib.Add(audioClip.name, audioClip);
            }


            //处理音频源，也就是得到用来播放声音的音乐播放器
            _AudioSourceArray = this.GetComponents<AudioSource>();
            _AudioSource_BackgroundAudio = gameObject.AddComponent<AudioSource>();               //其中一个用来播放背景音乐
            _AudioSource_AudioEffectA = gameObject.AddComponent<AudioSource>();                  //其中一个用来播放音乐1
            _AudioSource_AudioEffectB = gameObject.AddComponent<AudioSource>();


            //从数据持久化中得到音量数值
            // if (PlayerPrefs.GetFloat("AudioBackgroundVolumns") >= 0)
            // {
            //     AudioBackgroundVolumns = PlayerPrefs.GetFloat("AudioBackgroundVolumns");
            // }
            // else
            // {
                AudioBackgroundVolumns = 0.5f;
            // }
            // _AudioSource_BackgroundAudio.volume = AudioBackgroundVolumns;
            // if (PlayerPrefs.GetFloat("AudioEffectVolumns") >= 0)
            // {
            //     AudioEffectVolumns = PlayerPrefs.GetFloat("AudioEffectVolumns");
            //     
            // }
            // else
            // {
                AudioEffectVolumns = 1;
            // }
            _AudioSource_AudioEffectA.volume = AudioEffectVolumns;
            _AudioSource_AudioEffectB.volume = AudioEffectVolumns;
            //设置音乐的音效
            if (PlayerPrefs.HasKey("AudioBackgroundPitch"))
            {
                AudioBackgroundPitch = PlayerPrefs.GetFloat("AudioBackgroundPitch");
            }
            if (PlayerPrefs.HasKey("AudioEffectPitch"))
            {
                AudioEffectPitch = PlayerPrefs.GetFloat("AudioEffectPitch");
            }

            _AudioSource_BackgroundAudio.pitch = AudioBackgroundPitch;
            _AudioSource_AudioEffectA.pitch = AudioEffectPitch;
            _AudioSource_AudioEffectB.pitch = AudioEffectPitch;

        }

        /// <summary>
        /// 播放背景音乐
        /// 传入的参数是背景音乐的AudioClip
        /// </summary>
        /// <param name="audioClip">音频剪辑</param>
        public static void PlayBackground(AudioClip audioClip)
        {
            //防止背景音乐的重复播放。
            if (_AudioSource_BackgroundAudio.clip == audioClip)
            {
                return;
            }

            //处理全局背景音乐音量
            _AudioSource_BackgroundAudio.volume = AudioBackgroundVolumns;
            _AudioSource_BackgroundAudio.pitch = AudioBackgroundPitch;
            if (audioClip)
            {
                _AudioSource_BackgroundAudio.loop = true;                      //背景音乐是循环播放的
                _AudioSource_BackgroundAudio.clip = audioClip;
                _AudioSource_BackgroundAudio.Play();
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayBackground()] audioClip==null !");
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// 传入的参数是声音片段的名字，要注意，其声音片段要加入声音数组中
        /// </summary>
        /// <param name="strAudioName"></param>
        public static void PlayBackground(string strAudioName)
        {
            if (!string.IsNullOrEmpty(strAudioName))
            {
                PlayBackground(_DicAudioClipLib[strAudioName]);
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayBackground()] strAudioName==null !");
            }
        }

        /// <summary>
        /// 播放音效_音频源A
        /// </summary>
        /// <param name="audioClip">音频剪辑</param>
        public static void PlayAudioEffectA(AudioClip audioClip)
        {
            //处理全局音效音量
            _AudioSource_AudioEffectA.volume = AudioEffectVolumns;
            _AudioSource_AudioEffectA.pitch = AudioEffectPitch;

            if (audioClip)
            {
                _AudioSource_AudioEffectA.clip = audioClip;
                _AudioSource_AudioEffectA.Play();
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayAudioEffectA()] audioClip==null ! Please Check! ");
            }
        }
        /// <summary>
        /// 播放音效_音频源A
        /// </summary>
        /// <param name="strAudioEffctName">音效名称</param>
        public static void PlayAudioEffectA(string strAudioEffctName)
        {
            if (!string.IsNullOrEmpty(strAudioEffctName))
            {
                PlayAudioEffectA(_DicAudioClipLib[strAudioEffctName]);
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayAudioEffectA()] strAudioEffctName==null ! Please Check! ");
            }
        }
        /// <summary>
        /// 播放音效_音频源B
        /// </summary>
        /// <param name="audioClip">音频剪辑</param>
        public static void PlayAudioEffectB(AudioClip audioClip)
        {
            //处理全局音效音量
            _AudioSource_AudioEffectB.volume = AudioEffectVolumns;
            _AudioSource_AudioEffectB.pitch = AudioEffectPitch;
            if (audioClip)
            {
                _AudioSource_AudioEffectB.clip = audioClip;
                _AudioSource_AudioEffectB.Play();
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayAudioEffectB()] audioClip==null ! Please Check! ");
            }
        }

        /// <summary>
        /// 播放音效_音频源B
        /// </summary>
        /// <param name="strAudioEffctName">音效名称</param>
        public static void PlayAudioEffectB(string strAudioEffctName)
        {
            if (!string.IsNullOrEmpty(strAudioEffctName))
            {
                PlayAudioEffectB(_DicAudioClipLib[strAudioEffctName]);
            }
            else
            {
                Debug.LogWarning("[AudioManager.cs/PlayAudioEffectB()] strAudioEffctName==null ! Please Check! ");
            }
        }


        /// <summary>
        /// 停止播放音效A
        /// </summary>
        public static void StopPlayAudioEffectA()
        {
            _AudioSource_AudioEffectA.Stop();
        }

        /// <summary>
        /// 停止播放音效B
        /// </summary>
        public static void StopPlayAudioEffectB()
        {
            _AudioSource_AudioEffectB.Stop();
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public static void StopPlayAudioBackGround()
        {
            _AudioSource_BackgroundAudio.Stop();
        }

        /// <summary>
        /// 改变背景音乐音量
        /// </summary>
        /// <param name="floAudioBGVolumns"></param>
        public static void SetAudioBackgroundVolumns(float floAudioBGVolumns)
        {
            _AudioSource_BackgroundAudio.volume = floAudioBGVolumns;
            AudioBackgroundVolumns = floAudioBGVolumns;
            //数据持久化
            PlayerPrefs.SetFloat("AudioBackgroundVolumns", floAudioBGVolumns);
        }


        /// <summary>
        /// 改变音效音量
        /// </summary>
        /// <param name="floAudioEffectVolumns"></param>
        public static void SetAudioEffectVolumns(float floAudioEffectVolumns)
        {
            _AudioSource_AudioEffectA.volume = floAudioEffectVolumns;
            _AudioSource_AudioEffectB.volume = floAudioEffectVolumns;
            AudioEffectVolumns = floAudioEffectVolumns;
            //数据持久化
            PlayerPrefs.SetFloat("AudioEffectVolumns", floAudioEffectVolumns);
        }

        /// <summary>
        /// 改变背景音乐的音调
        /// </summary>
        /// <param name="floAudioBGPichs">改变的音调值</param>
        public static void SetAudioBackgroundPitch(float floAudioBGPitchs)
        {
            _AudioSource_BackgroundAudio.pitch = floAudioBGPitchs;

            //数据持久化
            PlayerPrefs.SetFloat("AudioBackgroundPitch", floAudioBGPitchs);
        }

        /// <summary>
        /// 改变音效的音调
        /// </summary>
        /// <param name="floAudioEffectPitchs">音效的音调值</param>
        public static void SetAudioEffectPitch(float floAudioEffectPitchs)
        {
            _AudioSource_AudioEffectA.pitch = floAudioEffectPitchs;
            _AudioSource_AudioEffectB.pitch = floAudioEffectPitchs;

            //数据持久化
            PlayerPrefs.SetFloat("AudioEffectPitch", floAudioEffectPitchs);
        }

    }
