LaserMod - Laser Frequency Modulation Depth
===========================================

Overview
--------

A standalone command line app to evaluate the frequency modulation depth of frequency stabilized lasers during their measurement with an optical frequency comb generator. A very specific setup is neccessary in order to use this app.

The main application is the calibration of He-Ne laser with an internal iodine cell, stabilized using the third harmonic detection technique according to the [Mise en pratique for the definition of the metre](https://www.bipm.org/en/publications/mises-en-pratique). Due to the working principle the laser radiation is frequency modulated. The peak-to-peack modulation width is an important parameter for the operation of this standards. The numerous methods proposed for measuring this parameter have their advantages and disadvantages. The method preseted here can be used during the actual frequency calibration without additional optical setup.

Preconditions
-------------

The only supported device is the Agilent/[Keysight](https://www.keysight.com/) 53230A Universal Frequency Counter. The very similar 53220A is not useable. The counter must be operated under [specific settings](#counter-settings) which are summarized below.

The input signal must be a sufficiently optimized beat signal between the laser to be tested and an optical frequency comb. The comb should be referenced by a source with good short time (< 1 s) stability. In the author's laboratory an active hydrogen maser works well while a standard Cs clock produces less reliably results. Referencing the comb to a high stability optical frequency is possible, also.

Since version 2.5 this app can also analyse the beat signal between two lasers (i.e. without frequency comb). Provided that the two modulation frequencies differ sufficiently, both modulation widths are evaluated. The app automatically detects which type of signal is present.

The data is recorded by the frequency counter on an external USB flash drive and evaluated offline using this app. A direct interface beween counter and computer is not currently planned.

The modulation frequency of the laser to be tested must be lower than approximately 10 kHz.

Usage
-----

The measurement data is stored on the USB flash drive as a simple CSV-file. No metadata, like counter settings, is stored in this file. Since the gate time is a absolutely necessary information, it must be encoded in the file name! The first integer (from the left) is interpreted as the gate time in µs, all subsequent numbers are ignored. The following examples show the usage of this syntax:

* `T10BEV1f_06` : gate time 10 µs
* `measurement22SIOS02-test` : gate time 22 µs
* `BEV2d_005ms` : gate time 2 µs - this was probably not intended!

The recorded file must be processed with the LaserMod app to obtain the frequency modulation width and additional parameters.

### Command Line Usage

```
LaserMod  [options] input-filename [output-filename]
```

### Options

`-v` : verbatim output.

`-n`: initial window size.

`--csv` : single output line useful for CSV files.

`--header` : static header CSV files. For laser vs comb, no input file necessary.

`--header2` : static header CSV files. For laser1 vs laser2, no input file necessary.

`--test` : single output line and file name parsing for validation purposes.

`--help` : as the name implies.

### Examples

```
LaserMod T10_CCL-K11_FSB_20240904_103049
```
Processes the file T10_CCL-K11_FSB_20240904_103049.csv and when successful, writes the results in file T10_CCL-K11_FSB_20240904_103049.prn in the current working directory. Only the value of the modulation width is displayed. The gate time is interpreted as 10 µs. The time stamp was added to the file name by the counter and is stored as a modified Julian date (MJD) in the result file.

```
LaserMod -v -n1000 gate5_BEV1d_01 BEV1mod01
```
Processes the file gate5_BEV1d_01.csv and when successful, writes the results in file BEV1mod01.prn in the current working directory. All evaluated parameters are displayed (verbatim). The gate time is interpreted as 5 µs.

Counter settings
----------------

Refer to the 53230A manual for details.

### Input
* Coupling: AC
* Impedance: 50 Ohm
* Range: 5 V
* BW Limit: Off
* Probe: None
* Level: 50 %, Pos, Auto On, Noise Rej Off

### Mode
* Totalize
* Gated
* Gate: 0.002 ms to 0.022 ms (0.010 ms prefered)
* Gate Src: Timed

### Data Log
* File Select: External (file name must encode the gate time!)
* t-Stamp: On (Off for shorter file names)
* Duration: Readings
* Set Count: 1 000 000

Mathematical background
-----------------------

The mathematical background of the evaluation method is scetched in the following. The actual implementation can be found in the source code.

The principle is based on recording the beat frequency at very short and regular intervals. The modulation depth is then derived from this data by fitting it to a sine function. This fitting process occurs in two steps. First, the modulation frequency is determined through Fourier analysis, and this value is fixed (and not fitted) for the actual sine fit. This method can be extended analogously to a beat signal between two frequency-modulated lasers. The software automatically detects which of the two scenarios is present.

An alternative method simply determines the standard deviation of the data for a singly modulated signal (statistical technique). For a purely sinusoidal frequency-modulated signal, the modulation depth is simply the square root of 2 times the standard deviation.

Both methods are not performed on the entire dataset (usually 1,000,000 points), but rather incrementally on smaller sections (windows).

Finally, a sinc correction compensates for the modulation depth attenuation (aperture effect) caused by the finite gating time.

### Empirical corrections

Three empirical corrections are applied to compensate for systematic offsets in the measurement and/or instrument. The actual values of the corrections are determined by calibration measurement with synthetic signals produced by a digital signal generator.
Respective correction values are stored in the static class `InstrumentConstants` and can be changed by the user if necessary.

#### Totalize correction

The raw totalize counter readings are on average smaller than the true value by an empirical offset of **0.286**.

#### Modulation width correction — statistical technique

For the statistical technique, the estimated raw peak-to-peak modulation width *M*pp is corrected by a linear term.

#### Modulation width correction — LSQ technique

For the least-squares (LSQ) technique, the estimated raw peak-to-peak modulation width *M*pp is corrected by a linear term and a quadratic term of *f*mod, where *f*mod is the modulation frequency.

Installation
------------

If you do not want to build the application from the source code you can use the released binaries. Just copy the .exe and the .dll files to a directory of your choice. This direcory should be included in the user's PATH variable.

Dependencies and Acknowledgments
--------------------------------

* [At.Matus.StatisticPod](https://github.com/matusm/At.Matus.StatisticPod)
* [Math.NET numerics](https://numerics.mathdotnet.com)

---

**Note**: This library is not officially affiliated with or endorsed by Keysight Technologies.

