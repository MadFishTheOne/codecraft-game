using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniGameInterfaces;
using Microsoft.Xna.Framework.Content;

namespace CoreNamespace
{
    public class Viewer:IDebug
    {
        public static Matrix ViewProj;
        public static Vector3 CameraPosition;
        ///// <summary>

        ///// I cant write it in GameVector. but this is needed method
        ///// </summary>
        ///// <param name="vec"></param>
        ///// <returns></returns>
        //internal static GameVector ToGameVector(GameVector vec)
        //{
        //    return new GameVector(vec.X, vec.Y);
        //}
        internal static Vector2 ToVector2(GameVector vec)
        {
            return new Vector2(vec.X, vec.Y);
        }
        static public Vector3[] TeamColors = { new Vector3(1,0,0),new Vector3(0,1,0),new Vector3(0,0,1),
                                          new Vector3(1,1,0),new Vector3(0,1,1),new Vector3(1,0,1)};
        SpriteFont font;
        public int screenHeight, screenWidth;
        public int ScreenHeight
        {
            get { return screenHeight; }
        }
        public int ScreenWidth
        { get { return screenWidth; } }
        SpriteBatch spriteBatch;
        ContentManager content;
        Texture2D DestroyerTexture, CorvetteTexture,
            CruiserTexture,
            DestroyerSmall, CorvetteSmall,
            CruiserSmall,
            LaserTexture,
            //EngineTexture,
            //MiniMapTexture,
            FirePunchTexture, EnvironmentTexture;
        internal GraphicsDeviceManager graphics;
        VertexDeclaration vertexDecl;
        #region vertex declaration
        public VertexElement[] VertexElements = new VertexElement[]
{
    //position
    new VertexElement(0, 0, VertexElementFormat.Vector3,
                            VertexElementMethod.Default, VertexElementUsage.Position, 0),
    //index
    new VertexElement(0, 12, VertexElementFormat.Single,
                             VertexElementMethod.Default,
                             VertexElementUsage.PointSize, 0),
    //texture coo
    new VertexElement(0, 12+4, VertexElementFormat.Vector2,
                             VertexElementMethod.Default,
                             VertexElementUsage.TextureCoordinate, 0),
};

        #endregion
        public struct VertexPositionIndexTexture
        {
            public Vector3 Position;
            public float Index;
            public GameVector TexCoo;
        }
        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        Effect shipEffect, blowEffect, shotEffect, environmentEffect,debugFigureEffect;
        public Viewer(int ScreenWidth, int ScreenHeight, ContentManager Content, GraphicsDeviceManager Graphics)
        {
            this.screenHeight = ScreenHeight;
            this.screenWidth = ScreenWidth;
            content = Content;
            graphics = Graphics;
            CRectanglesInBatch = 0;
        }
        void CreateBuffers()
        {
            vertexDecl = new VertexDeclaration(graphics.GraphicsDevice, VertexElements);
            VertexPositionIndexTexture[] vertexData = new VertexPositionIndexTexture[256 * 6];
            for (int i = 0; i < 256; i++)
            {
                vertexData[i * 6 + 0].Position = new Vector3(1, 1, 0);
                vertexData[i * 6 + 1].Position = new Vector3(1, -1, 0);
                vertexData[i * 6 + 2].Position = new Vector3(-1, -1, 0);
                vertexData[i * 6 + 3].Position = new Vector3(-1, -1, 0);
                vertexData[i * 6 + 4].Position = new Vector3(-1, 1, 0);
                vertexData[i * 6 + 5].Position = new Vector3(1, 1, 0);
                vertexData[i * 6 + 0].TexCoo = new GameVector(1, 1);
                vertexData[i * 6 + 1].TexCoo = new GameVector(1, 0);
                vertexData[i * 6 + 2].TexCoo = new GameVector(0, 0);
                vertexData[i * 6 + 3].TexCoo = new GameVector(0, 0);
                vertexData[i * 6 + 4].TexCoo = new GameVector(0, 1);
                vertexData[i * 6 + 5].TexCoo = new GameVector(1, 1);
                vertexData[i * 6 + 0].Index = i;
                vertexData[i * 6 + 1].Index = i;
                vertexData[i * 6 + 2].Index = i;
                vertexData[i * 6 + 3].Index = i;
                vertexData[i * 6 + 4].Index = i;
                vertexData[i * 6 + 5].Index = i;
            }
            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, vertexData.Length * vertexDecl.GetVertexStrideSize(0),
                BufferUsage.None);
            vertexBuffer.SetData<VertexPositionIndexTexture>(vertexData);
            indexBuffer = new IndexBuffer(graphics.GraphicsDevice, 16 * vertexData.Length,
                 BufferUsage.None, IndexElementSize.SixteenBits);
            UInt16[] indexData = new ushort[vertexData.Length];
            for (ushort i = 0; i < indexData.Length; i++)
                indexData[i] = i;
            indexBuffer.SetData<UInt16>(indexData);
        }
        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            font = content.Load<SpriteFont>("testfont");
            DestroyerTexture = content.Load<Texture2D>("textures\\DestroyerTexture");
            CorvetteTexture = content.Load<Texture2D>("textures\\CorvetteTexture");
            CruiserTexture = content.Load<Texture2D>("textures\\CruiserTexture");
            DestroyerSmall = content.Load<Texture2D>("textures\\DestroyerSmall");
            CorvetteSmall = content.Load<Texture2D>("textures\\CorvetteSmall");
            CruiserSmall = content.Load<Texture2D>("textures\\CruiserSmall");
            LaserTexture = content.Load<Texture2D>("textures\\LaserTexture");
            EnvironmentTexture = content.Load<Texture2D>("textures\\EnvironmentTexture");
            //EngineTexture = content.Load<Texture2D>("textures\\EngineTexture");
            //MiniMapTexture = content.Load<Texture2D>("textures\\MinimapTexture");
            FirePunchTexture = content.Load<Texture2D>("textures\\BlowTexture");
            shipEffect = content.Load<Effect>("effects\\ShipEffect");
            blowEffect = content.Load<Effect>("effects\\BlowEffect");
            shotEffect = content.Load<Effect>("effects\\ShotEffect");
            debugFigureEffect = content.Load<Effect>("effects\\DebugFigureEffect");
            environmentEffect = content.Load<Effect>("effects\\EnvironmentEffect");
            CreateBuffers();
        }
        const int BlowDetalization = 5;//billboards count in one blow            
        const int MaxBatchSize = 240;
        public void DrawEnvironment()
        {
            Vector4[] param = new Vector4[1];
            param[0].X = 0;
            param[0].Y = 0;
            param[0].Z = 0;
            param[0].W = 0;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
            graphics.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.One;
            graphics.GraphicsDevice.VertexDeclaration = vertexDecl;
            graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vertexDecl.GetVertexStrideSize(0));
            graphics.GraphicsDevice.Indices = indexBuffer;
            environmentEffect.Parameters["ViewProj"].SetValue(ViewProj);
            environmentEffect.Parameters["PlayerColors"].SetValue(TeamColors);
            environmentEffect.Parameters["tex"].SetValue(EnvironmentTexture);
            environmentEffect.Parameters["Positions"].SetValue(param);
            environmentEffect.Parameters["Size"].SetValue(new Vector2(30000, 30000));
            environmentEffect.Begin();
            EffectPass p = environmentEffect.CurrentTechnique.Passes[0];
            p.Begin();
            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1 * 6, 0, 1 * 2);
            p.End();
            environmentEffect.End();
        }
        /// <summary>
        /// format: (position.X;position.Y;RotationAngle;Playerowner)
        /// </summary>
        Vector4[] CorvetteBatchParams = new Vector4[MaxBatchSize];
        /// <summary>
        /// format: (position.X;position.Y;RotationAngle;Playerowner)
        /// </summary>
        Vector4[] DestroyerBatchParams = new Vector4[MaxBatchSize];
        /// <summary>
        /// format: (position.X;position.Y;RotationAngle;Playerowner)
        /// </summary>
        Vector4[] CruiserBatchParams = new Vector4[MaxBatchSize];
        /// <summary>
        /// format: (position.X;position.Y;radius;time)
        /// </summary>
        Vector4[] BlowBatchParams = new Vector4[MaxBatchSize];

        /// <summary>
        /// rectangle format: (center.x; center.y; Size.x; Size.y)
        ///                   (Angle;  0 ; 0 ;Color)
        /// circle  format: (center.x; center.y; Diameter; Diameter)
        ///                   (0;  1 ; 0 ;Color)
        /// </summary>
        Vector4[] DebugRectangleBatchParams1 = new Vector4[MaxBatchSize/2];
        Vector4[] DebugRectangleBatchParams2 = new Vector4[MaxBatchSize/2];
        int CRectanglesInBatch;

        public void DrawUnits(List<Unit> units)
        {
            graphics.GraphicsDevice.RenderState.DepthBufferEnable = false;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
            graphics.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            shipEffect.Parameters["ViewProj"].SetValue(ViewProj);
            shipEffect.Parameters["PlayerColors"].SetValue(TeamColors);
            blowEffect.Parameters["ViewProj"].SetValue(ViewProj);
            blowEffect.Parameters["tex"].SetValue(FirePunchTexture);
            blowEffect.Parameters["Detalization"].SetValue(BlowDetalization);
            graphics.GraphicsDevice.VertexDeclaration = vertexDecl;
            graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vertexDecl.GetVertexStrideSize(0));
            graphics.GraphicsDevice.Indices = indexBuffer;
            
            int CDestroyersInBatch = 0;
            int CCorvettesInBatch = 0;
            int CCruisersInBatch = 0;
            int CBlowsInBatch = 0;
            int currUnit = 0;
            while (currUnit < units.Count)
            {
                if (units[currUnit].HP < 0)
                {
                    for (int i = 0; i < BlowDetalization; i++)
                    {
                        BlowBatchParams[CBlowsInBatch].X = units[currUnit].position.X;
                        BlowBatchParams[CBlowsInBatch].Y = units[currUnit].position.Y;
                        BlowBatchParams[CBlowsInBatch].Z = units[currUnit].BlowRadius;
                        BlowBatchParams[CBlowsInBatch].W = (float)units[currUnit].timeAfterDeath / (float)units[currUnit].maxTimeAfterDeath;
                        CBlowsInBatch++;
                        if (CBlowsInBatch == MaxBatchSize) DrawBlowBatch(BlowBatchParams, ref CBlowsInBatch);
                    }
                }
                else
                {
                    if (units[currUnit].ShipType == ShipTypes.Destroyer)
                    {
                        DestroyerBatchParams[CDestroyersInBatch].X = units[currUnit].position.X;
                        DestroyerBatchParams[CDestroyersInBatch].Y = units[currUnit].position.Y;
                        DestroyerBatchParams[CDestroyersInBatch].Z = units[currUnit].RotationAngle;
                        DestroyerBatchParams[CDestroyersInBatch].W = units[currUnit].PlayerOwner;
                        CDestroyersInBatch++;
                        if (CDestroyersInBatch == MaxBatchSize) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture, DestroyerSmall, Core.DestroyerSize, ShipTypes.Destroyer);
                    }
                    if (units[currUnit].ShipType == ShipTypes.Corvette)
                    {
                        CorvetteBatchParams[CCorvettesInBatch].X = units[currUnit].position.X;
                        CorvetteBatchParams[CCorvettesInBatch].Y = units[currUnit].position.Y;
                        CorvetteBatchParams[CCorvettesInBatch].Z = units[currUnit].RotationAngle;
                        CorvetteBatchParams[CCorvettesInBatch].W = units[currUnit].PlayerOwner;
                        CCorvettesInBatch++;
                        if (CCorvettesInBatch == MaxBatchSize) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture, CorvetteSmall, Core.CorvetteSize, ShipTypes.Corvette);
                    }
                    if (units[currUnit].ShipType == ShipTypes.Cruiser)
                    {
                        CruiserBatchParams[CCruisersInBatch].X = units[currUnit].position.X;
                        CruiserBatchParams[CCruisersInBatch].Y = units[currUnit].position.Y;
                        CruiserBatchParams[CCruisersInBatch].Z = units[currUnit].RotationAngle;
                        CruiserBatchParams[CCruisersInBatch].W = units[currUnit].PlayerOwner;
                        CCruisersInBatch++;
                        if (CCruisersInBatch == MaxBatchSize) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture, CruiserSmall, Core.CruiserSize, ShipTypes.Cruiser);
                    }
                }
                currUnit++;
            }
            if (CDestroyersInBatch > 0) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture, DestroyerSmall, Core.DestroyerSize, ShipTypes.Destroyer);
            if (CCorvettesInBatch > 0) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture, CorvetteSmall, Core.CorvetteSize, ShipTypes.Corvette);
            if (CCruisersInBatch > 0) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture, CruiserSmall, Core.CruiserSize, ShipTypes.Cruiser);
            if (CBlowsInBatch > 0) DrawBlowBatch(BlowBatchParams, ref CBlowsInBatch);
            DrawDebugRectangleBatch();
            foreach (Unit unit in units)
            {
                if ((unit.Text != null) && (unit.Text != ""))
                {
                    Vector4 position = new Vector4(unit.position.X, unit.position.Y, 0, 1);
                    position = Vector4.Transform(position, ViewProj);
                    position /= position.W;
                    position.Y = -position.Y;
                    position += Vector4.One;
                    position *= 0.5f;
                    position.X *= screenWidth;
                    position.Y *= screenHeight;
                    DrawText(unit.Text, new GameVector(position.X, position.Y), 0, Microsoft.Xna.Framework.Graphics.Color.White);
                }
            }
        }
        private void DrawBlowBatch(Vector4[] BlowBatchParams, ref int CBlowsInBatch)
        {
            blowEffect.Begin();
            blowEffect.Parameters["Params"].SetValue(BlowBatchParams);
            EffectPass p = blowEffect.CurrentTechnique.Passes[0];
            p.Begin();
            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CBlowsInBatch * 6, 0, CBlowsInBatch * 2);
            p.End();
            CBlowsInBatch = 0;
            blowEffect.End();
        }
        void DrawUnitBatch(Vector4[] UnitInstanceParams, ref int CUnits, Texture2D Text, Texture2D TextSmall, GameVector Size, ShipTypes type)
        {
            float BigLength = 3000;
            shipEffect.Begin();
            float SizeMultiplier = 0;
            EffectPass p = shipEffect.CurrentTechnique.Passes[0];
            shipEffect.Parameters["Positions"].SetValue(UnitInstanceParams);
            if (CameraPosition.Z > BigLength)
            {
                shipEffect.Parameters["tex"].SetValue(TextSmall);
                switch (type)
                {
                    case ShipTypes.Corvette: SizeMultiplier = 2.2f; break;
                    case ShipTypes.Cruiser: SizeMultiplier = 1.6f; break;
                    case ShipTypes.Destroyer: SizeMultiplier = 3.2f; break;
                }
                SizeMultiplier *= CameraPosition.Z / BigLength;
                shipEffect.Parameters["Size"].SetValue(ToVector2(Size * SizeMultiplier));
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CUnits * 6, 0, CUnits * 2);
                p.End();
            }
            if (CameraPosition.Z > BigLength) SizeMultiplier = 1.2f * CameraPosition.Z / BigLength;
            else SizeMultiplier = 1.2f;
            shipEffect.Parameters["Size"].SetValue(ToVector2(Size * SizeMultiplier));
            shipEffect.Parameters["tex"].SetValue(Text);
            p.Begin();
            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CUnits * 6, 0, CUnits * 2);
            p.End();
            CUnits = 0;
            shipEffect.End();
        }
        private void DrawShotBatch(Vector4[] ShotBatchParams1, Vector3[] ShotBatchParams2, ref int CShotsInBatch)
        {
            shotEffect.Begin();
            shotEffect.Parameters["tex"].SetValue(LaserTexture);
            shotEffect.Parameters["Positions"].SetValue(ShotBatchParams1);
            shotEffect.Parameters["Params"].SetValue(ShotBatchParams2);
            EffectPass p = shotEffect.CurrentTechnique.Passes[0];
            p.Begin();
            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CShotsInBatch * 6, 0, CShotsInBatch * 2);
            p.End();
            CShotsInBatch = 0;
            shotEffect.End();
        }
        internal void DrawShots(Shots shots)
        {
            shotEffect.Parameters["ViewProj"].SetValue(ViewProj);
            Vector4[] ShotBatchParams1 = new Vector4[MaxBatchSize / 2];
            Vector3[] ShotBatchParams2 = new Vector3[MaxBatchSize / 2];
            int CShotsInBatch = 0;
            foreach (Shots.Shot shot in shots)
            {
                ShotBatchParams1[CShotsInBatch].X = shot.pos.X;
                ShotBatchParams1[CShotsInBatch].Y = shot.pos.Y;
                ShotBatchParams1[CShotsInBatch].Z = shot.End.X;
                ShotBatchParams1[CShotsInBatch].W = shot.End.Y;
                ShotBatchParams2[CShotsInBatch].X = 1;// (shot.hitSomebody) ? 1 : -1;
                ShotBatchParams2[CShotsInBatch].Y = 0;
                ShotBatchParams2[CShotsInBatch].Z = shot.Size;//width
                CShotsInBatch++;
                if (CShotsInBatch >= MaxBatchSize / 2)
                    DrawShotBatch(ShotBatchParams1, ShotBatchParams2, ref CShotsInBatch);
            }
            if (CShotsInBatch > 0)
                DrawShotBatch(ShotBatchParams1, ShotBatchParams2, ref CShotsInBatch);
        }
        public void DrawText(string text, GameVector pos, int align, Microsoft.Xna.Framework.Graphics.Color color)
        {
            spriteBatch.Begin();
            Vector2 fontOrigin = new Vector2(0.0f, 0.0f); ;
            switch (align)
            {
                case 1:
                    fontOrigin = new Vector2(font.MeasureString(text).X / 2, 0.0f);
                    break;
                case 2:
                    fontOrigin = new Vector2(font.MeasureString(text).X, 0.0f);
                    break;
            }
            spriteBatch.DrawString(font, text, ToVector2(pos + new GameVector(2, 2)), Microsoft.Xna.Framework.Graphics.Color.Black, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(font, text, ToVector2(pos), color, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }


        internal void Update()
        {
            Viewer.ViewProj = Matrix.CreateLookAt(CameraPosition, new Vector3(CameraPosition.X, CameraPosition.Y, 0), new Vector3(0, 1, 0)) *
                 Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)screenWidth / (float)screenHeight, 10, 100000);
        }


        public void DrawDebugRectangleBatch()
        {
            if (CRectanglesInBatch > 0)
            {
                debugFigureEffect.Begin();
                EffectPass p = debugFigureEffect.CurrentTechnique.Passes[0];
                debugFigureEffect.Parameters["Params1"].SetValue(DebugRectangleBatchParams1);
                debugFigureEffect.Parameters["Params2"].SetValue(DebugRectangleBatchParams2);
                debugFigureEffect.Parameters["ViewProj"].SetValue(ViewProj);
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CRectanglesInBatch * 6, 0, CRectanglesInBatch * 2);
                p.End();
                debugFigureEffect.End();
                CRectanglesInBatch = 0;
            }
        }
        #region IDebug Members

        public void DrawRectangle(MiniGameInterfaces.Rectangle Rectangle, MiniGameInterfaces.Color Color)
        {
            if (CRectanglesInBatch < MaxBatchSize / 2)
             {
                DebugRectangleBatchParams1[CRectanglesInBatch].X = Rectangle.Center.X;
                DebugRectangleBatchParams1[CRectanglesInBatch].Y = Rectangle.Center.Y;
                DebugRectangleBatchParams1[CRectanglesInBatch].Z = Rectangle.Size.X;
                DebugRectangleBatchParams1[CRectanglesInBatch].W = Rectangle.Size.Y;
                DebugRectangleBatchParams2[CRectanglesInBatch].X = Rectangle.Angle;
                DebugRectangleBatchParams2[CRectanglesInBatch].Y = 0;//is rectangle
                DebugRectangleBatchParams2[CRectanglesInBatch].Z = 0;//reserved
                DebugRectangleBatchParams2[CRectanglesInBatch].W =ToFloat(Color);
                CRectanglesInBatch++;
            }
        }
        /// <summary>
        /// tool function to pass color in shader as one float parameter. Reduces quality to 10^3 colors
        /// </summary>
        /// <param name="Color"></param>
        /// <returns></returns>
        private float ToFloat(MiniGameInterfaces.Color Color)
        {
            return (float)((Math.Round(Color.r*0.94f, 1) +
                Math.Round(Color.g * 0.94f, 1) * 10 + Math.Round(Color.b * 0.94f, 1) * 100 + Math.Round(Color.a * 0.94f, 1) * 1000));
        }
        public void DrawCircle(Circle Circle, MiniGameInterfaces.Color Color)
        {
            if (CRectanglesInBatch < MaxBatchSize / 2)
            {
                DebugRectangleBatchParams1[CRectanglesInBatch].X = Circle.Center.X;
                DebugRectangleBatchParams1[CRectanglesInBatch].Y = Circle.Center.Y;
                DebugRectangleBatchParams1[CRectanglesInBatch].Z = Circle.Radius*2;
                DebugRectangleBatchParams1[CRectanglesInBatch].W = Circle.Radius * 2;
                DebugRectangleBatchParams2[CRectanglesInBatch].X = 0;
                DebugRectangleBatchParams2[CRectanglesInBatch].Y = 1;//is circle
                DebugRectangleBatchParams2[CRectanglesInBatch].Z = 0;//reserved
                DebugRectangleBatchParams2[CRectanglesInBatch].W = ToFloat(Color);
                CRectanglesInBatch++;
            }
        }

        public void DrawPoint(GameVector Vector, MiniGameInterfaces.Color Color)
        {
            if (CRectanglesInBatch < MaxBatchSize / 2)
            {
                DebugRectangleBatchParams1[CRectanglesInBatch].X = Vector.X;
                DebugRectangleBatchParams1[CRectanglesInBatch].Y = Vector.Y;
                DebugRectangleBatchParams1[CRectanglesInBatch].Z = 0.01f*CameraPosition.Z;
                DebugRectangleBatchParams1[CRectanglesInBatch].W = 0.01f * CameraPosition.Z;
                DebugRectangleBatchParams2[CRectanglesInBatch].X = 0;
                DebugRectangleBatchParams2[CRectanglesInBatch].Y = 1;//is circle
                DebugRectangleBatchParams2[CRectanglesInBatch].Z = 0;//reserved
                DebugRectangleBatchParams2[CRectanglesInBatch].W = ToFloat(Color);
                CRectanglesInBatch++;
            }
        }

        public void DrawLine(Line Line,MiniGameInterfaces.Color Color)
        {
            if (CRectanglesInBatch < MaxBatchSize / 2)
            {
                MiniGameInterfaces.Rectangle rect=new MiniGameInterfaces.Rectangle((Line.pt1+Line.pt2)*0.5f,
                    new GameVector(Line.Length(),0.003f*CameraPosition.Z),Line.pt2-Line.pt1);
                DrawRectangle(rect, Color);
            }
        }

        #endregion
    }
}
