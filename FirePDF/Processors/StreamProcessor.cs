﻿using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public class StreamProcessor : IStreamProcessor
    {
        private IRenderer renderer;
        private GraphicsStateProcessor gsp;
        private LineProcessor lp;
        private PaintingProcessor pp;
        private ClippingProcessor cp;
        private ImageProcessor ip;
        private TextProcessor tp;

        private RecursiveStreamReader streamReader;

        public StreamProcessor(IRenderer renderer)
        {
            this.renderer = renderer;
        }

        public void didStartReadingStream()
        {
            renderer.willStartRenderingStream(streamReader);
        }

        public void processOperation(Operation operation)
        {
            gsp.processOperation(operation);
            pp.processOperation(operation);
            cp.processOperation(operation);
            lp.processOperation(operation);
            ip.processOperation(operation);
            tp.processOperation(operation);
        }
       
        public void willFinishReadingPage()
        {
            
        }

        public void willFinishReadingStream()
        {
            
        }

        public void willStartReadingPage(RecursiveStreamReader parser)
        {
            streamReader = parser;
            
            gsp = new GraphicsStateProcessor(streamReader);
            lp = new LineProcessor();
            pp = new PaintingProcessor(renderer, lp);
            cp = new ClippingProcessor(gsp.getCurrentState, lp);
            ip = new ImageProcessor(streamReader, renderer);
            tp = new TextProcessor(gsp.getCurrentState, streamReader, renderer);

            renderer.willStartRenderingPage(gsp.getCurrentState);
        }
    }
}
