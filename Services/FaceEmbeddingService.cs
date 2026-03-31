using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

public class FaceEmbeddingService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _yunetPath;
    private readonly string _sfacePath;

    public FaceEmbeddingService(IWebHostEnvironment env)
    {
        _env = env;
        _yunetPath = Path.Combine(_env.ContentRootPath, "AIModels", "face_detection_yunet_2023mar.onnx");
        _sfacePath = Path.Combine(_env.ContentRootPath, "AIModels", "face_recognition_sface_2021dec.onnx");

        if (!File.Exists(_yunetPath))
            throw new FileNotFoundException("YuNet model not found.", _yunetPath);

        if (!File.Exists(_sfacePath))
            throw new FileNotFoundException("SFace model not found.", _sfacePath);
    }

    public float[] ExtractEmbedding(string imagePath)
    {
        using var image = Cv2.ImRead(imagePath);

        if (image.Empty())
            throw new Exception("Cannot read image.");

        var faceRow = DetectSingleFace(image);
        if (faceRow == null)
            throw new Exception("No face detected or multiple faces detected.");

        using (faceRow)
        {
            using var croppedFace = CropFace(image, faceRow);
            using var resizedFace = new Mat();
            Cv2.Resize(croppedFace, resizedFace, new OpenCvSharp.Size(112, 112));

            var inputTensor = ConvertMatToTensor(resizedFace);

            using var session = new InferenceSession(_sfacePath);

            var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("data", inputTensor)
        };

            using var results = session.Run(inputs);
            var output = results.First(x => x.Name == "fc1").AsEnumerable<float>().ToArray();

            return NormalizeVector(output);
        }
    }

    private Mat? DetectSingleFace(Mat image)
    {
        using var detector = FaceDetectorYN.Create(
            _yunetPath,
            "",
            new OpenCvSharp.Size(image.Width, image.Height),
            scoreThreshold: 0.9f,
            nmsThreshold: 0.3f,
            topK: 5000
        );

        using var faces = new Mat();
        var count = detector.Detect(image, faces);

        if (count != 1 || faces.Rows != 1)
            return null;

        return faces.Row(0).Clone();
    }

    private Mat CropFace(Mat image, Mat faceRow)
    {
        float x = faceRow.At<float>(0, 0);
        float y = faceRow.At<float>(0, 1);
        float w = faceRow.At<float>(0, 2);
        float h = faceRow.At<float>(0, 3);

        int left = Math.Max(0, (int)x);
        int top = Math.Max(0, (int)y);
        int width = Math.Min(image.Width - left, (int)w);
        int height = Math.Min(image.Height - top, (int)h);

        if (width <= 0 || height <= 0)
            throw new Exception("Invalid face bounding box.");

        var rect = new Rect(left, top, width, height);
        return new Mat(image, rect).Clone();
    }

    private DenseTensor<float> ConvertMatToTensor(Mat image)
    {
        if (image.Channels() != 3)
            throw new Exception("Image must have 3 channels.");

        var tensor = new DenseTensor<float>(new[] { 1, 3, 112, 112 });

        for (int y = 0; y < 112; y++)
        {
            for (int x = 0; x < 112; x++)
            {
                Vec3b pixel = image.At<Vec3b>(y, x);

                float b = pixel.Item0 / 255.0f;
                float g = pixel.Item1 / 255.0f;
                float r = pixel.Item2 / 255.0f;

                tensor[0, 0, y, x] = r;
                tensor[0, 1, y, x] = g;
                tensor[0, 2, y, x] = b;
            }
        }

        return tensor;
    }

    private float[] NormalizeVector(float[] vector)
    {
        float norm = 0f;
        for (int i = 0; i < vector.Length; i++)
        {
            norm += vector[i] * vector[i];
        }

        norm = (float)Math.Sqrt(norm);
        if (norm < 1e-8f)
            return vector;

        var normalized = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            normalized[i] = vector[i] / norm;
        }

        return normalized;
    }

    public float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new Exception("Vector length mismatch.");

        float dot = 0f;
        float normA = 0f;
        float normB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dot / ((float)(Math.Sqrt(normA) * Math.Sqrt(normB)) + 1e-8f);
    }
}