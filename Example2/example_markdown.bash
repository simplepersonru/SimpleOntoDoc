SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR/.." || exit 1

SIMPLEDOC_INPUT_PATH="$SCRIPT_DIR/schema.hjson" \
SIMPLEDOC_TITLE="Example2 Markdown Ontology" \
SIMPLEDOC_DESCRIPTION="Minimal diverse HJSON example for Markdown generation" \
SIMPLEDOC_OUTPUT_PATH="$SCRIPT_DIR/output" \
SIMPLEDOC_MARKDOWN_RENDER=true \
dotnet run --no-launch-profile --project SimpleOntoDoc.csproj
