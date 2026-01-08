import { createClient } from "@/lib/supabase/server";
// import { type EmailOtpType } from "@supabase/supabase-js";
import { redirect } from "next/navigation";
import { type NextRequest } from "next/server";

export async function GET(request: NextRequest) {
  // This route is for Supabase email confirmation - currently disabled
  // TODO: Implement email confirmation with new auth system
  redirect("/auth/error?error=Email confirmation is being migrated to the new authentication system");
}
