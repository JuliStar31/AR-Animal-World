mobilenetv2-12.onnx

MobileNetV2 image classifier trained on ImageNet (1000 classes), from the
ONNX Model Zoo (https://github.com/onnx/models, validated/vision/
classification/mobilenet, Apache License 2.0). The weights come from
torchvision (BSD 3-Clause License).

Input "input": 1x3x224x224 RGB, scaled to 0..1, then normalised with
mean (0.485, 0.456, 0.406) and std (0.229, 0.224, 0.225).
Output "output": 1x1000 class scores (before softmax).

Used by Assets/Scripts/AI/AnimalPictureRecognizer.cs, which looks at the
classes lion (291), tusker (101), Indian elephant (385), African elephant
(386), wood rabbit (330), hare (331) and Angora (332).

sha256 c0c3f76d93fa3fd6580652a45618618a220fced18babf65774ed169de0432ad5
