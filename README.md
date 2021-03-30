BlazeFaceBarracuda
------------------

This repository contains an incomplete implementation of BlazeFace on Unity Barracuda.

For some reasons, the current implementation of Barracuda is not compatible with BlazeFace.
The root cause of the problem is still under the investigation.

Google Colab notebook (this works correctly) - https://colab.research.google.com/drive/17EftUSSW5qdbEpbokU9lxJVKcWvqDSQH?authuser=1

### Why it fails

There are two issues in the current implementation of Barracuda:

- In the ONNX specification, the Pad operator takes a pads input as a tensor input, but Barracuda tries to take it from attributes.
- Barracuda only supports padding along the spatial axis (W, H, and D), but BlazeFace uses padding along the C axis.

TODO: Report these issues to the dev.
