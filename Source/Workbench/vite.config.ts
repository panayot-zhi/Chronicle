// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defineConfig } from 'vite';
import react from "@vitejs/plugin-react";
import path from 'path';
import { VitePluginEmitMetadata } from './ViteEmitMetadataPlugin';

export default defineConfig({
    build: {
        outDir: './wwwroot',
        assetsDir: '',
        rollupOptions: {
            external: [
                './backup',
            ],
        },
    },
    plugins: [
        react({

            babel: {

                plugins: [
                    'typewind/babel',
                    [
                        '@babel/plugin-proposal-decorators',
                        { legacy: true }
                    ]
                ]
            }
        }),
        VitePluginEmitMetadata() as any
    ],
    server: {
        port: 9000,
        open: false,
        proxy: {
            '/api': {
                target: 'http://localhost:8080',
                ws: true
            },
            '/swagger': {
                target: 'http://localhost:8080'
            }
        }
    },
    resolve: {
        alias: {
            'API': path.resolve('./API'),
            'MVVM': path.resolve('./MVVM'),
            'assets': path.resolve('./assets'),
            'Components': path.resolve('./Components'),
            'Layout': path.resolve('./Layout'),
            'Features': path.resolve('./Features'),
            'Strings': path.resolve('./Strings'),
        }
    }
});
