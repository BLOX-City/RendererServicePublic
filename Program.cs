using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Microsoft.Extensions.Primitives;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Globalization;
using ThumbHashes;
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

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
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
                response.Headers!.ContentType = "application/json";
                return;
            }            

            bool didCreateShirtUri = Uri.TryCreate(shirt == null ? String.Empty : shirt, UriKind.Absolute, out Uri? shirtUri);
            bool didCreatePantsUri = Uri.TryCreate(pants == null ? String.Empty : pants, UriKind.Absolute, out Uri? pantsUri);

            if (!didCreateShirtUri && shirt != null)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_SHIRT_TEXTURE_INVALID_FORMAT\"],\"detailed\":[\"shirt texture uri was invalid its either a file:// http://\"]}}");
                response.Headers!.ContentType = "application/json";
                return;
            }

            if (!didCreatePantsUri && pants != null)
            {
                response.StatusCode = 400;
                await response.WriteAsync("{\"meta\":{\"status\":false,\"messages\":[\"E_PARAM_PANTS_TEXTURE_INVALID_FORMAT\"],\"detailed\":[\"pants texture uri was invalid its either a file:// http://\"]}}");
                response.Headers!.ContentType = "application/json";
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
            lqipBitmap = lqipBitmap.Resize(new SKImageInfo(3, 3), SKSamplingOptions.Default);

            var thumbhash = ThumbHash.FromImage(3, 3, lqipBitmap.GetPixelSpan());

            response.Headers!["X-BLOXCity-Lqip"] = Convert.ToBase64String(thumbhash.Hash.ToArray()).Replace("=", String.Empty).Replace('/', '`');

            response.Headers!.ContentType = "image/webp";
            await response.Body.WriteAsync(rotated.Encode(SKEncodedImageFormat.Webp, 75).ToArray());

            Renderer.Rendered.Remove(id, out _);
        });
        webApplication.RunAsync("http://0.0.0.0:1444");
        GameWin game = new GameWin(400, 600, "test");
        game.Run();
    }
}