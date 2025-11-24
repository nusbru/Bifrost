import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Disable cacheComponents for authenticated dynamic routes
  cacheComponents: false,
  // Enable standalone output for Docker
  output: "standalone",
};

export default nextConfig;
