
"../Audio/weh.wav" => string fileName;

me.sourceDir() + fileName => string filePath;
SndBuf buf => PitShift p => dac;
filePath => buf.read;

float noteToShift;
1 => noteToShift;
Math.pow(2, noteToShift / 12) => buf.rate;

1::second => now;
