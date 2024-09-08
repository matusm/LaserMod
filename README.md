LaserMod - Laser Frequency Modulation Depth
===========================================

Overview
--------

A standalone command line app to evaluate the frequency modulation depth of frequency stabilized lasers during their measurement with an optical frequency comb generator. A very specific setup is neccessary in order to use this app.

The main application is in the calibration of He-Ne laser with an internal iodine cell, stabilized using the third harmonic detection technique according to the [Mise en pratique for the definition of the metre](https://www.bipm.org/en/publications/mises-en-pratique). Due to the working principle the laser radiation is frequency modulated. The peak-to-peack modulation width is an important parameter for the operation of this standards. The numerous methods proposed for measuring this parameter have their advantages and disadvantages. This method can be used during the actual frequency calibration without additional optical setup.

Preconditions
-------------

The only supported device is the Agilent/[Keysight](https://www.keysight.com/) 53230A Universal Frequency Counter. The very similar 53220A is not useable. The counter must be operated under [specific settings](#counter-settings) which are summarized below.

The input signal must be a sufficiently optimized beat signal between the laser to be tested and an optical frequency comb. The comb should be referenced by a source with high short time (< 1 s) stability. In the author's laboratory an active hydrogen maser works well while a standard Cs clock produces less reliably results. Referencing the comb to a high stability optical frequency is possible, also.

The data is recorded by the frequency counter on an external USB storage device and evaluated offline using this app. A direct interface beween counter and computer is not currently planned.

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

`--test` : special option for numerical tests.

`--help` : as the name implies.

### Examples

```
LaserMod T10_CCL-K11_FSB_20240904_103049
```
Processes the file T10_CCL-K11_FSB_20240904_103049.csv and when successful, writes the results in file T10_CCL-K11_FSB_20240904_103049.prn in the current working directory. Only the value of the modulation width is displayed. The gate time is interpreted as 10 µs. The time stamp was added to the file name by the counter.

```
LaserMod -v -n1000 gate5_BEV1d_01 BEV1mod01
```
Processes the file gate5_BEV1d_01.csv and when successful, writes the results in file BEV1mod01.prn in the current working directory. All evaluated parameters are displayed (verbatim). The gate time is interpreted as 5 µs.

Counter settings
----------------

Refer to the A53230A manual on how to modify settings.

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
* Gate: 0.001 ms to 0.022 ms (0.010 ms prefered)
* Gate Src: Timed

### Data Log
* File Select: External (file name must encode the gate time!)
* t-Stamp: On (Off for shorter file names)
* Duration: Readings
* Set Count: 1 000 000

Mathematical background
-----------------------

TBA

### Empirical corrections

TBA

Dependencies
------------

Math.Net Numerics: https://github.com/mathnet/mathnet-numerics

At.Matus.StatisticPod: https://github.com/matusm/At.Matus.StatisticPod
