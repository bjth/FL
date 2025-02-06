using System.Numerics;
using Arch.Core;
using FL.Client.Providers;
using FL.Client.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int screenWidth = 1920;
const int screenHeight = 1080;

//Setup services.
var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole());

//Setup world and Library Managers
services.AddSingleton(World.Create());
services.AddScoped<WindowProvider>();
services.AddSingleton<DeltaTimeProvider>();

//Setup Systems.
services.AddGameSystem<DeltaTimeSystem>();
services.AddGameSystem<InputSystem>();

services.AddGameSystem<GameStateSystem>();
services.AddGameSystem<DrawSystem>();

//Setup Signals and Consumers
services.AddMessagePipe();
var provider = services.BuildServiceProvider();

SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.Msaa4xHint | ConfigFlags.UndecoratedWindow);
InitWindow(screenWidth, screenHeight, "Forgotten Legacy");
SetTargetFPS(240);

await using (var asyncScope = provider.CreateAsyncScope())
using (_ = asyncScope.ServiceProvider.GetRequiredService<World>())
{
    //Create our Camera. This should probably be handled in the Service Provider.
    Camera3D camera = new()
    {
        Position = new Vector3(0.0f, 10.0f, 10.0f), // Camera position
        Target = new Vector3(0.0f, 0.0f, 0.0f), // Camera looking at point
        Up = new Vector3(0.0f, 1.0f, 0.0f), // Camera up vector (rotation towards target)
        FovY = 45.0f, // Camera field-of-view Y
        Projection = CameraProjection.Perspective // Camera mode type
    };
    
    // Camera settings
    var cameraDistance = 10f;     // Distance from the camera to the player
    const float cameraHeight = 5f;        // Height of the camera
    const float rotationSpeed = 0.00005f;    // Rotation speed for the camera
    // Spherical coordinates for camera rotation
    var theta = 0f;  // Azimuthal angle (around the Y-axis)
    var phi = MathF.PI / 4f;  // Polar angle (from the vertical Y-axis)

    var cubePosition = new Vector3(0.0f, 0.5f, 0.0f);
    var cubeSize = new Vector3(1.0f, 1.0f, 1.0f);
    
    var planePosition = new Vector3(0.0f, -0.5f, 0.0f);
    var planeSize = new Vector3(500.0f, 0.5f, 500.0f);
    
    var targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    var speed = 1.2f;

    var model = LoadModel("Assets/player.glb");
    var bounds = new BoundingBox();
    unsafe
    {
        bounds = GetMeshBoundingBox(model.Meshes[0]);    
    }
    

    var gameSystems = asyncScope.ServiceProvider.GetServices<IGameSystem>().ToList();
    foreach (var gameSystem in gameSystems)
    {
        await gameSystem.InitializeAsync();
    }

    while (!WindowShouldClose())
    {
        var deltaTime = asyncScope.ServiceProvider.GetRequiredService<DeltaTimeProvider>().DeltaTime;
        
        // Update camera position based on mouse X and Y movement for rotation
        if (IsMouseButtonDown(MouseButton.Left))
        {
            // Rotate the camera using the mouse's movement
            float mouseX = GetMouseX();
            float mouseY = GetMouseY();

            // Calculate delta movement of the mouse
            var deltaX = mouseX - GetScreenWidth() / 2f;
            var deltaY = mouseY - GetScreenHeight() / 2f;

            // Adjust the rotation angles based on mouse movement
            theta -= deltaX * rotationSpeed;
            phi -= deltaY * rotationSpeed;

            // Clamp the phi (polar angle) so it doesn't go upside down
            phi = MathF.Max(0.1f, MathF.Min(MathF.PI - 0.1f, phi));
        }

        // Zoom in/out using the mouse wheel
        cameraDistance -= GetMouseWheelMove() * 2f; // Adjust zoom speed (e.g., 2f)
        cameraDistance = Math.Clamp(cameraDistance, 5f, 50f); // Set limits for zooming

        // Update camera position based on camera distance and height
        // camera.Position = new Vector3(
        //     cubePosition.X + cameraDistance * MathF.Sin(phi) * MathF.Cos(theta),
        //     cubePosition.Y + cameraHeight + cameraDistance * MathF.Cos(phi),
        //     cubePosition.Z + cameraDistance * MathF.Sin(phi) * MathF.Sin(theta)
        // );
        //
        // // Keep the camera looking at the player (target)
        // camera.Target = cubePosition;

        // Update camera
        UpdateCamera(ref camera, CameraMode.Custom);

        if (IsMouseButtonPressed(MouseButton.Right))
        {
            var ray = GetScreenToWorldRay(GetMousePosition(), camera);

            var groundBoundingBox = new BoundingBox(
                new Vector3(planePosition.X - planeSize.X / 2, planePosition.Y - planeSize.Y / 2,
                    planePosition.Z - planeSize.Z / 2), new
                    Vector3(planePosition.X + planeSize.X / 2, planePosition.Y + planeSize.Y / 2,
                        planePosition.Z + planeSize.Z / 2));

            var rayCollision = GetRayCollisionBox(ray, groundBoundingBox);
            
            //var collision = RayExtensions.RayPlaneIntersection(ray, planePosition, new Vector3(0f, 1f, 0f));
            //if (collision.HasValue)
            if (rayCollision.Hit)
            {
                Console.WriteLine("Position {0}", rayCollision.Point);
                targetPosition.Y = rayCollision.Point.Y + cubeSize.Y/2;
                targetPosition.X = rayCollision.Point.X;
                targetPosition.Z = rayCollision.Point.Z;
            }
        }

        // Calculate the direction vector from the cube to the target
        Vector3 direction = targetPosition - cubePosition;

        // If the cube is far enough from the target, move it
        if (Vector3.Distance(cubePosition, targetPosition) > 0.1f)
        {
            // Perform linear interpolation (lerp) for smoother movement
            cubePosition = Vector3.Lerp(cubePosition, targetPosition, speed * deltaTime);
        }
        else
        {
            // Snap the cube to the target once it's close enough
            cubePosition = targetPosition;
        }
        
        foreach (var gameSystem in gameSystems)
        {
            await gameSystem.UpdateAsync();
        }
        
        BeginDrawing();
        ClearBackground(GetColor(0x000000FF));
        BeginMode3D(camera);
        
        //Temp Debug
        //DrawCube(cubePosition, cubeSize.X, cubeSize.Y, cubeSize.Z, Color.Red);
        DrawModel(model, cubePosition, 0.01f, Color.Red);
        DrawModelWires(model, cubePosition, 0.01f, Color.Maroon);
        //DrawCubeWires(cubePosition, cubeSize.X, cubeSize.Y, cubeSize.Z, Color.Maroon);
        DrawCube(planePosition, planeSize.X, planeSize.Y, planeSize.Z, Color.DarkGray);
        //DrawGrid(5000, 1.0f);
        //Temp End
        
        foreach (var gameSystem in gameSystems)
        {
            await gameSystem.DrawAsync();
        }
        EndMode3D();
        
        foreach (var gameSystem in gameSystems)
        {
            await gameSystem.DrawUIAsync();
        }
        
        EndDrawing();
    }
}


CloseWindow();