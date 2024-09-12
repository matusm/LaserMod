Test files by synthesizer
=========================

These test files were obtained with the usual BEV setup. The signal source was a Agilent 33250A Arbitrary Waveform Generator. This device can produce traceable frequency modulated signals similar to the beat of the comb with a laser. The carrier frequency, the modulation frequency and the modulation width are known and can be used to validate this app.

To use the `--test` option these values must be encoded in the file name according to the following syntax:

`Ttt_ffff_m` where `tt` is the gate time in µs, `ffff` the modulation frequency in Hz and `m` the modulation width in MHz.