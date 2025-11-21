import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Disable cacheComponents for authenticated dynamic routes
  cacheComponents: false,
};

export default nextConfig;
