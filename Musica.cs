using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace FaceTrackingBasics
{
    public class Musica
    {
        private DirectSoundOut output;
        private BlockAlignReductionStream stream;
        private WaveStream pcm;
        private string[] cancons;
        private int index;

        public Musica(string[] cancons)
        {
            this.output = null;
            this.stream = null;
            this.pcm = null;
            this.cancons = cancons;
            this.index = 0;
        }

        /// <summary>
        /// reprodueix la seguent canço
        /// </summary>
        public void next()
        {
            if (this.output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                {
                    this.output.Stop();
                    this.output.Dispose();
                }
            }

            if (this.index == this.cancons.Length - 1) this.index = 0;
            else this.index++;

            escoltarCanco(this.index);
        }

        public void replay()
        {
            if (this.output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                {
                    this.output.Stop();
                    this.output.Dispose();
                }
            }
            escoltarCanco(this.index);
        }

        /// <summary>
        /// Metode per escoltar musica
        /// </summary>
        /// <param name="index">posició de la canço</param>
        public void escoltarCanco(int index)
        {
                if (this.cancons[index].EndsWith(".mp3"))
                {
                    this.pcm = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(this.cancons[index]));
                    this.stream = new NAudio.Wave.BlockAlignReductionStream(pcm);
                }
                else if (this.cancons[index].EndsWith(".wav"))
                {
                    this.pcm = new WaveChannel32(new WaveFileReader(this.cancons[index]));
                    this.stream = new NAudio.Wave.BlockAlignReductionStream(pcm);
                }
                else throw new InvalidOperationException("Not a correct audio file type.");
            this.output = new DirectSoundOut();
            this.output.Init(stream);
            this.output.Play();
        }

        /// <summary>
        /// puja o baixa el volumen
        /// </summary>
        /// <param name="volume">float 0 fins 1</param>
        public void setVolume(float volume)
        {
            this.output.Volume = volume;
        }
    }
}
