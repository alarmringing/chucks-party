
"../Audio/weh.wav" => string fileName;

me.sourceDir() + fileName => string filePath;
SndBuf buf => NRev rev => dac;
filePath => buf.read;
1 => buf.rate;
.25 => rev.mix;
0.3 => buf.gain;
1.5 => buf.rate;
0 => buf.pos;

1::second => now;