using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Microsoft.Extensions.Primitives;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Globalization;
namespace BLOXCityRenderer;

public class GameWin : GameWindow {
    public GameWin(int width, int height, string windowTitle) : base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        ClientSize = (width, height),
        Title = windowTitle,
        Vsync = VSyncMode.Off,
        TransparentFramebuffer = true
    }) {

    }

    float x = 0;
    float y = 0;
    float prevX = 0;
    float prevY = 0;
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        /*Renderer.World.ActiveCamera.Rotation = new Vector3(379.79922f, 370.79922f, 0);
        Renderer.World.ActiveCamera.Position = new Vector3(0.8740163f, -3.7181618f, -3.7042923f);*/
        /*
        x = MousePosition.X;
        y = MousePosition.Y;
        
        Renderer.World.ActiveCamera.Rotation = new Vector3(Renderer.World.ActiveCamera.Pitch, Renderer.World.ActiveCamera.Yaw, Renderer.World.ActiveCamera.Roll);

        float deltaYaw = x - prevX;
        float deltaPitch = prevY - y;
        float mouseSensitivity = 0.9f;
        Renderer.World.ActiveCamera.Yaw += deltaYaw * mouseSensitivity;
        Renderer.World.ActiveCamera.Pitch -= deltaPitch * mouseSensitivity;
        if(KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
            Renderer.World.ActiveCamera.Move(new Vector3(-0.025f, 0.025f, 0.025f));
        }
        if(KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
            Renderer.World.ActiveCamera.Move(new Vector3(0.025f, 0.025f, -0.025f));
        }        
        if(KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
            Renderer.World.ActiveCamera.Move(new Vector3(-0.25f, 0.25f, -0.25f));
        }
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.G))
        {
            Console.WriteLine($"Location Captured Yaw: {Renderer.World.ActiveCamera.Yaw} Pitch: {Renderer.World.ActiveCamera.Pitch} Position X: {Renderer.World.ActiveCamera.Position.X} Positiion Y: {Renderer.World.ActiveCamera.Position.Y} Position Z: {Renderer.World.ActiveCamera.Position.Z}");
        }    
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
        {
            Close();
        }

        prevX = MousePosition.X;
        prevY = MousePosition.Y;
        // MousePosition = new Vector2(ClientLocation.X - ClientSize.X, ClientLocation.Y);
        */


    }


    protected override void OnLoad()
    {
        base.OnLoad();
        Renderer.Initialize(this.ClientSize.X, this.ClientSize.Y);
        CenterWindow();
    }

    protected override unsafe void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);

        bool didGetRenderOperation = Renderer.RenderOperations.TryPop(out RenderOperation? renderOperation);

        if (!didGetRenderOperation)
        {
            return;
        }
        else
        {
            if (renderOperation!.RenderOperationType == RenderOperationType.Fullbody)
            {
                Renderer.SetFullbodyCamera();
            }
            else if (renderOperation.RenderOperationType == RenderOperationType.Headshot)
            {
                Renderer.SetHeadshotCamera();
            }
            
            if (renderOperation.Shirt != null)
            {
                Renderer.LoadShirt(renderOperation.Shirt);
            }
            else
            {
                Renderer.ResetShirt();
            }

            if (renderOperation.Pants != null)
            {
                Renderer.LoadPants(renderOperation.Pants);
            }
            else
            {
                Renderer.ResetPants();
            }

            Renderer.ResetColor();

            if (renderOperation.HeadColor != null)
            {
                Renderer.SetColor(CharacterPart.Head, renderOperation.HeadColor!.Value);
            }            

            if (renderOperation.TorsoColor != null)
            {
                Renderer.SetColor(CharacterPart.Torso, renderOperation.TorsoColor!.Value);
            }

            if (renderOperation.LeftArmColor != null)
            {
                Renderer.SetColor(CharacterPart.LeftArm, renderOperation.LeftArmColor!.Value);
            }            

            if (renderOperation.RightArmColor != null)
            {
                Renderer.SetColor(CharacterPart.RightArm, renderOperation.RightArmColor!.Value);
            }            

            if (renderOperation.LeftLegColor != null)
            {
                Renderer.SetColor(CharacterPart.LeftLeg, renderOperation.LeftLegColor!.Value);
            }            

            if (renderOperation.RightLegColor != null)
            {
                Renderer.SetColor(CharacterPart.RightLeg, renderOperation.RightLegColor!.Value);
            }

            if (renderOperation.Hat != null)
            {
                Renderer.LoadHat(renderOperation.Hat);
                Console.WriteLine(renderOperation.Hat);
            }

            Renderer.Render();

            byte[] imageData = Renderer.GetImageBytes();


            SKBitmap bitmap = new SKBitmap(new SKImageInfo(400, 800, SKColorType.Rgba8888));
            Marshal.Copy(imageData, 0, (IntPtr)bitmap.GetPixels(), imageData.Length);
            Renderer.Rendered[renderOperation.Id].SetResult(bitmap);



            Renderer.UnloadMeshes();
            Renderer.UnloadTextures();

            SwapBuffers();
        }
        
        
    }
}

public static class Program {
    public static void Main() {
        WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
        WebApplication webApplication = webApplicationBuilder.Build();
        webApplication.MapGet("/", async (HttpRequest request, HttpResponse response) =>
        {

            Guid id = Guid.NewGuid();
            string? shirt = request.Query["Shirt"];
            string? headColor = request.Query["Head:Color"];
            string? torsoColor = request.Query["Torso:Color"];
            string? lArmColor = request.Query["LArm:Color"];
            string? rArmColor = request.Query["RArm:Color"];
            string? lLegColor = request.Query["LLeg:Color"];
            string? rLegColor = request.Query["RLeg:Color"];
            string? pants = request.Query["Pants"];
            string? hat = request.Query["Hat"];
            
            RenderOperationType renderOperationType = (RenderOperationType)int.Parse(request.Query["RenderOperationType"].FirstOrDefault("0")!);

            bool hasNoShirtColors = lArmColor == null && rArmColor == null && torsoColor == null;
            bool hasNoPantsColors = lLegColor == null && rLegColor == null;

            if (shirt == null && hasNoShirtColors)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_SHIRT_TEXTURE_NULL\"],\"detailed\":[\"shirt texture uri cannot be null expected format file:// or https:// over the network\"]}}");
                return;
            }

            if (pants == null && hasNoPantsColors)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_PANTS_TEXTURE_NULL\"],\"detailed\":[\"pants texture uri cannot be null expected format file:// or https:// over the network\"]}}");
                return;
            }            

            bool didCreateShirtUri = Uri.TryCreate(shirt == null ? String.Empty : shirt, UriKind.Absolute, out Uri? shirtUri);
            bool didCreatePantsUri = Uri.TryCreate(pants == null ? String.Empty : pants, UriKind.Absolute, out Uri? pantsUri);

            if (!didCreateShirtUri && shirt != null)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_SHIRT_TEXTURE_INVALID_FORMAT\"],\"detailed\":[\"shirt texture uri was invalid its either a file:// http://\"]}}");
                return;
            }

            if (!didCreatePantsUri && pants != null)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_PANTS_TEXTURE_INVALID_FORMAT\"],\"detailed\":[\"pants texture uri was invalid its either a file:// http://\"]}}");
                return;
            }


            RenderOperation renderOperation = new RenderOperation()
            {
                Id = id,
                RenderOperationType = renderOperationType
                  
            };


            if (shirtUri != null)
            {
                renderOperation.Shirt = await Asset.Load(shirtUri);
            }

            if (pantsUri != null) {
                renderOperation.Pants = await Asset.Load(pantsUri);
            }

            if (torsoColor != null && torsoColor.Length > 1)
            {
                bool didParseTorsoColor = uint.TryParse(torsoColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexTorsoColor);

                if (didParseTorsoColor)
                {
                    renderOperation.TorsoColor = hexTorsoColor;
                }
            }

            if (lArmColor != null && lArmColor.Length > 1)
            {
                bool didParseLeftArmColor = uint.TryParse(lArmColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexLeftArmColor);

                if (didParseLeftArmColor)
                {
                    renderOperation.LeftArmColor = hexLeftArmColor;
                }
            }            

            if (rArmColor != null && rArmColor.Length > 1)
            {
                bool didParseRightArmColor = uint.TryParse(rArmColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexRightArmColor);

                if (didParseRightArmColor)
                {
                    renderOperation.RightArmColor = hexRightArmColor;
                }
            }                        

            if (rLegColor != null && rLegColor.Length > 1)
            {
                bool didParseRightLegColor = uint.TryParse(rLegColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexRightLegColor);

                if (didParseRightLegColor)
                {
                    renderOperation.RightLegColor = hexRightLegColor;
                }
            }                                    

            if (lLegColor != null && lLegColor.Length > 1)
            {
                bool didParseLeftLegColor = uint.TryParse(rLegColor!.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexLeftLegColor);

                if (didParseLeftLegColor)
                {
                    renderOperation.LeftLegColor = hexLeftLegColor;
                }
            }                                                

            if (rLegColor != null && rLegColor.Length > 1)
            {
                bool didParseRightLegColor = uint.TryParse(rLegColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexRightLegColor);

                if (didParseRightLegColor)
                {
                    renderOperation.RightLegColor = hexRightLegColor;
                }
            }                                                

            if (headColor != null && headColor.Length > 1)
            {
                bool didParseHeadHexColor = uint.TryParse(headColor.Substring(1), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hexHeadColor);

                if (didParseHeadHexColor)
                {
                    renderOperation.HeadColor = hexHeadColor;
                }
            }
            /*string userId = request.Query["UserId"];
            string pants = request.Query["Pants"];
            string shirt = request.Query["Shirt"];

            request.Query.TryGetValue("Hat", out StringValues hatValues);

            string hat = hatValues.First();*/

            if (hat != null)
            {
                renderOperation.Hat = await Asset.Load(new Uri(hat.ToString(), UriKind.Absolute));
            }

            Renderer.RenderOperations.Push(renderOperation);



            
            Renderer.Rendered[id] = new TaskCompletionSource<SkiaSharp.SKBitmap>();

            SKBitmap bitmap = await Renderer.Rendered[id].Task;

            SKBitmap rotated = new SKBitmap(bitmap.Width, 500);

            SKCanvas canvas = new SKCanvas(rotated);

            canvas.Translate(0, rotated.Height);
            canvas.Scale(1, -1);
            canvas.DrawBitmap(bitmap, 0, 0);

            var lqipBitmap = rotated.Copy();
            lqipBitmap = lqipBitmap.Resize(new SKImageInfo(3, 2), SKSamplingOptions.Default);

            var _1 = lqipBitmap.GetPixel(0, 0);
            var _2 = lqipBitmap.GetPixel(1, 0);
            var _3 = lqipBitmap.GetPixel(2, 0);
            var _4 = lqipBitmap.GetPixel(0, 1);
            var _5 = lqipBitmap.GetPixel(0, 2);
            var _6 = lqipBitmap.GetPixel(0, 6);

            var averageColorR = _1.Red + _2.Red + _3.Red + _4.Red + _5.Red + _6.Red / 6;
            var averageColorG = _1.Green + _2.Green + _3.Green + _4.Green + _5.Green + _6.Green / 6;
            var averageColorB = _1.Blue + _2.Blue + _3.Blue + _4.Blue + _5.Blue + _6.Blue / 6;

            (float L, float a, float b) = OklabConverter.RgbToOklab(averageColorR / 255.0f, averageColorG / 255.0f, averageColorB / 255.0f);


            // oklabBits
            var (ll, aaa, bbb) = OklabConverter.FindOklabBits(L, a, b);
            var (baseL, baseA, baseB) = OklabConverter.BitsToLab(ll, aaa, bbb);

            (float L, float a, float b)[] cells = new (float L, float a, float b)[6];

            cells[0] = OklabConverter.RgbToOklab(_1.Red, _1.Green, _1.Blue);
            cells[1] = OklabConverter.RgbToOklab(_2.Red, _2.Green, _2.Blue) ;
            cells[2] = OklabConverter.RgbToOklab(_3.Red, _3.Green, _3.Blue); 
            cells[3] = OklabConverter.RgbToOklab(_4.Red, _4.Green, _4.Blue); 
            cells[4] = OklabConverter.RgbToOklab(_5.Red, _5.Green, _5.Blue); 
            cells[5] = OklabConverter.RgbToOklab(_6.Red, _6.Green, _6.Blue);

            float[] luminance = new float[6];

            for (int i = 0; i < cells.Length; i++)
            {
                luminance[i] = MathF.Min(1, Math.Max(0, 0.5f + cells[i].L - baseL));
            }

              byte ca = (byte)Math.Round(luminance[0] * 0b11);
              byte cb = (byte)Math.Round(luminance[1] * 0b11);
              byte cc = (byte)Math.Round(luminance[2] * 0b11);
              byte cd = (byte)Math.Round(luminance[3] * 0b11);
              byte ce = (byte)Math.Round(luminance[4] * 0b11);
              byte cf = (byte)Math.Round(luminance[5] * 0b11);
              int lqip =
                -(2 << 19) +
                ((ca & 0b11) << 18) +
                ((cb & 0b11) << 16) +
                ((cc & 0b11) << 14) +
                ((cd & 0b11) << 12) +
                ((ce & 0b11) << 10) +
                ((cf & 0b11) << 8) +
                ((ll & 0b11) << 6) +
                ((aaa & 0b111) << 3) +
                (bbb & 0b111);

            response.Headers!["X-BLOXCity-Lqip"] = lqip.ToString();

            response.Headers!.ContentType = "image/webp";
            await response.Body.WriteAsync(rotated.Encode(SKEncodedImageFormat.Webp, 75).ToArray());

            Renderer.Rendered.Remove(id, out _);
        });
        webApplication.RunAsync("http://127.0.0.1:1444");
        GameWin game = new GameWin(400, 600, "test");
        game.Run();
    }
}