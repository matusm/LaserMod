LaserMod - Measure the Frequency Modulation Depth
=================================================

## Overview

A standalone command line app to evaluate  the portable optometer P-9710 by [Gigahertz-Optik GmbH](https://www.gigahertz-optik.com/) via its RS232 interface.

Its main usage is to perform photo current measurements for a predetermined number of samples. This process can be triggered manually whenever needed. A timestamp, average value, dispersion parameters and the corresponding photometric quantity value is logged in a file.

## Command Line Usage

```
LaserMod input-filename [output-filename] [options]
```

### Options

`-v` : verbatim output.

`-n` : Evaluation length in periods.

`--test` : specia option for numerical tests.

`--help` : as the name implies.

### Examples

```
LaserMod -n100 -p"/dev/tty.usbserial-FTY594BQ" --comment="test-mac"
```
Use 20 samples per measurement, write a comment in the log file, and use the given serial port. The log file has the default name `optometer.log` and is located in the current working directory.

```
optometer --port="COM3" --logfile="C:\temp\MyLogFile.txt"
```
Use 10 samples per measurement (default) and use the given serial port. The full path of the log file is given. If the directory `C:\temp` does not exist, the programm will crash.


## Log File Entries

On start the identifications and parameters of both, the instrument and the connected detector, are queried and logged. An important parameter is the detector sensitivity factor which is stored in the detector's EEPROM. This sensitivity factor is used to calculate the photometric quantity value from the measured photo current. The mesurement uncertainty of the sensitivity factor is not accessible in the EEPROM, thus the uncertainty of the photometric quantity can not be estimated. The value following the symbol ± is the standard uncertainty originating from the combined standard uncertainty as discussed below.

* Average value: arithmetic mean of the *n* photo current readings, in nA.                 

* The instrument measurement range for this average value

* Standard deviation: The standard deviation of the *n* photo current readings, in nA. (Not the standard deviation of the mean!)

* Specification uncertainty: The specification given by the manufacturer for the actual average photo current. It is stated as a standard uncertainty. 

* Combined standard uncertainty: combination of the standard deviation and the specification uncertainty.

* Photometric value: calculated value using the detector sensitivity factor


## Dependencies

Bev.Instruments.P9710: https://github.com/matusm/Bev.Instruments.P9710

At.Matus.StatisticPod: https://github.com/matusm/At.Matus.StatisticPod

CommandLineParser: https://github.com/commandlineparser/commandline 
