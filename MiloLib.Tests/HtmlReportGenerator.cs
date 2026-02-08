using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiloLib.Tests;

/// <summary>
/// Generates an HTML report showing test results as an interactive heatmap grid.
/// </summary>
public static class HtmlReportGenerator
{
    public static string GenerateReport(string outputPath)
    {
        var allResults = TestResultCollector.GetAllResults();
        var html = new StringBuilder();

        // Collect all revisions across all assets
        var allRevisions = new SortedSet<ushort>();
        foreach (var assetResults in allResults.Values)
            foreach (var rev in assetResults.Keys)
                allRevisions.Add(rev);

        // Compute per-asset stats
        int totalTests = 0, passedTests = 0, failedTests = 0, skippedTests = 0;
        foreach (var assetResults in allResults.Values)
        {
            foreach (var result in assetResults.Values)
            {
                totalTests++;
                switch (result.Status)
                {
                    case TestResultCollector.TestStatus.Passed: passedTests++; break;
                    case TestResultCollector.TestStatus.Failed: failedTests++; break;
                    case TestResultCollector.TestStatus.Skipped: skippedTests++; break;
                }
            }
        }

        double passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

        // Build JSON data for the client
        var jsonSb = new StringBuilder();
        jsonSb.Append('[');
        bool firstAsset = true;
        foreach (var (assetName, revisions) in allResults.OrderBy(kvp => kvp.Key))
        {
            if (!firstAsset) jsonSb.Append(',');
            firstAsset = false;

            int ap = 0, af = 0, ask = 0;
            foreach (var r in revisions.Values)
            {
                switch (r.Status)
                {
                    case TestResultCollector.TestStatus.Passed: ap++; break;
                    case TestResultCollector.TestStatus.Failed: af++; break;
                    case TestResultCollector.TestStatus.Skipped: ask++; break;
                }
            }

            jsonSb.Append($"{{\"name\":{JsonEscape(assetName)},\"p\":{ap},\"f\":{af},\"s\":{ask},\"revs\":{{");
            bool firstRev = true;
            foreach (var (rev, result) in revisions.OrderBy(kvp => kvp.Key))
            {
                if (!firstRev) jsonSb.Append(',');
                firstRev = false;
                int status = result.Status switch
                {
                    TestResultCollector.TestStatus.Passed => 0,
                    TestResultCollector.TestStatus.Failed => 1,
                    TestResultCollector.TestStatus.Skipped => 2,
                    _ => -1
                };
                string err = result.ErrorMessage ?? "";
                jsonSb.Append($"\"{rev}\":[{status},{JsonEscape(err)}]");
            }
            jsonSb.Append("}}");
        }
        jsonSb.Append(']');

        var revArray = string.Join(",", allRevisions);

        html.Append($@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>MiloLib Round-Trip Test Report</title>
<style>
*{{box-sizing:border-box;margin:0;padding:0}}
body{{font-family:'Segoe UI',system-ui,sans-serif;background:#1a1a2e;color:#e0e0e0;padding:16px}}
a{{color:#64b5f6}}
h1{{font-size:20px;font-weight:600;color:#fff;margin-bottom:4px}}
.subtitle{{color:#888;font-size:13px;margin-bottom:16px}}
.stats{{display:flex;gap:12px;margin-bottom:16px;flex-wrap:wrap}}
.stat{{background:#16213e;border-radius:8px;padding:12px 16px;min-width:120px}}
.stat .label{{font-size:11px;color:#888;text-transform:uppercase;letter-spacing:.5px}}
.stat .val{{font-size:22px;font-weight:700;margin-top:2px}}
.stat .val.green{{color:#4caf50}}.stat .val.red{{color:#ef5350}}.stat .val.orange{{color:#ffa726}}.stat .val.blue{{color:#42a5f5}}
.controls{{display:flex;gap:12px;margin-bottom:16px;flex-wrap:wrap;align-items:center}}
.search{{background:#16213e;border:1px solid #333;border-radius:6px;padding:8px 12px;color:#e0e0e0;font-size:14px;width:260px;outline:none}}
.search:focus{{border-color:#64b5f6}}
.search::placeholder{{color:#555}}
.btn-group{{display:flex;gap:0}}
.btn{{background:#16213e;border:1px solid #333;padding:6px 14px;font-size:12px;color:#aaa;cursor:pointer;transition:all .15s}}
.btn:first-child{{border-radius:6px 0 0 6px}}.btn:last-child{{border-radius:0 6px 6px 0}}
.btn.active{{background:#1e3a5f;color:#64b5f6;border-color:#64b5f6}}
.btn:hover{{background:#1e2d4a}}
.sort-select{{background:#16213e;border:1px solid #333;border-radius:6px;padding:6px 10px;color:#e0e0e0;font-size:12px;outline:none;cursor:pointer}}
.grid-wrap{{background:#16213e;border-radius:8px;overflow:hidden}}
.grid-header{{display:flex;align-items:center;position:sticky;top:0;z-index:10;background:#0f1629;border-bottom:1px solid #333;font-size:10px;color:#888}}
.grid-header .name-col{{width:220px;min-width:220px;padding:6px 10px;font-weight:600}}
.grid-header .rev-cols{{display:flex;flex:1;overflow:hidden}}
.grid-header .rev-col{{width:18px;min-width:18px;text-align:center;padding:4px 0}}
.grid-header .stats-col{{width:100px;min-width:100px;padding:6px 8px;text-align:right;font-weight:600}}
.asset-row{{display:flex;align-items:stretch;border-bottom:1px solid #1a1a2e;cursor:pointer;transition:background .1s}}
.asset-row:hover{{background:#1a2744}}
.asset-row.hidden{{display:none}}
.name-col{{width:220px;min-width:220px;padding:8px 10px;font-size:13px;font-weight:500;display:flex;align-items:center;gap:6px}}
.name-col .expand-icon{{font-size:10px;color:#555;transition:transform .15s;width:12px}}
.name-col .expand-icon.open{{transform:rotate(90deg)}}
.rev-cols{{display:flex;flex:1;align-items:center;overflow:hidden}}
.cell{{width:18px;min-width:18px;height:22px;margin:1px 0}}
.cell.p{{background:#2e7d32}}.cell.f{{background:#c62828}}.cell.s{{background:#e65100}}.cell.n{{background:transparent}}
.cell:hover{{opacity:.75}}
.stats-col{{width:100px;min-width:100px;padding:8px;text-align:right;font-size:12px;color:#888;display:flex;align-items:center;justify-content:flex-end;gap:4px}}
.pass-pct{{font-weight:600}}
.pass-pct.perfect{{color:#4caf50}}.pass-pct.good{{color:#8bc34a}}.pass-pct.mid{{color:#ffa726}}.pass-pct.bad{{color:#ef5350}}
.detail-panel{{display:none;background:#0f1629;border-bottom:1px solid #333;padding:12px 16px;font-size:12px}}
.detail-panel.open{{display:block}}
.detail-panel table{{width:100%;border-collapse:collapse}}
.detail-panel th{{text-align:left;padding:4px 8px;color:#888;font-size:11px;border-bottom:1px solid #222}}
.detail-panel td{{padding:4px 8px;border-bottom:1px solid #1a1a2e}}
.detail-panel .err{{color:#ef9a9a;max-width:600px;word-break:break-word;font-family:monospace;font-size:11px;white-space:pre-wrap}}
.legend{{display:flex;gap:16px;margin-bottom:12px;font-size:12px;color:#888;align-items:center}}
.legend-item{{display:flex;align-items:center;gap:4px}}
.legend-swatch{{width:14px;height:14px;border-radius:2px}}
.count-badge{{font-size:11px;padding:1px 6px;border-radius:10px;font-weight:600}}
.count-badge.p-badge{{background:#1b3a1b;color:#4caf50}}.count-badge.f-badge{{background:#3a1b1b;color:#ef5350}}.count-badge.s-badge{{background:#3a2a0f;color:#ffa726}}
#no-match{{display:none;text-align:center;padding:40px;color:#555;font-size:14px}}
</style>
</head>
<body>

<h1>MiloLib Round-Trip Serialization Test Report</h1>
<p class=""subtitle"">Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss} &mdash; {allResults.Count} asset types, revisions 0&ndash;{(allRevisions.Count > 0 ? allRevisions.Max : 0)}</p>

<div class=""stats"">
  <div class=""stat""><div class=""label"">Total Tests</div><div class=""val blue"">{totalTests}</div></div>
  <div class=""stat""><div class=""label"">Passed</div><div class=""val green"">{passedTests}</div></div>
  <div class=""stat""><div class=""label"">Failed</div><div class=""val red"">{failedTests}</div></div>
  <div class=""stat""><div class=""label"">Skipped</div><div class=""val orange"">{skippedTests}</div></div>
  <div class=""stat""><div class=""label"">Pass Rate</div><div class=""val green"">{passRate:F1}%</div></div>
</div>

<div class=""legend"">
  <span>Legend:</span>
  <div class=""legend-item""><div class=""legend-swatch"" style=""background:#2e7d32""></div>Passed</div>
  <div class=""legend-item""><div class=""legend-swatch"" style=""background:#c62828""></div>Failed</div>
  <div class=""legend-item""><div class=""legend-swatch"" style=""background:#e65100""></div>Skipped</div>
</div>

<div class=""controls"">
  <input type=""text"" class=""search"" id=""search"" placeholder=""Search asset types..."" oninput=""applyFilters()"">
  <div class=""btn-group"">
    <button class=""btn active"" data-filter=""all"" onclick=""setFilter(this)"">All</button>
    <button class=""btn"" data-filter=""failures"" onclick=""setFilter(this)"">Has Failures</button>
    <button class=""btn"" data-filter=""perfect"" onclick=""setFilter(this)"">All Passed</button>
    <button class=""btn"" data-filter=""skipped"" onclick=""setFilter(this)"">Has Skipped</button>
  </div>
  <select class=""sort-select"" id=""sort"" onchange=""applySort()"">
    <option value=""name"">Sort: Name</option>
    <option value=""failures"">Sort: Most Failures</option>
    <option value=""passrate"">Sort: Pass Rate</option>
    <option value=""skipped"">Sort: Most Skipped</option>
  </select>
</div>

<div class=""grid-wrap"" id=""grid"">
  <div class=""grid-header"">
    <div class=""name-col"">Asset Type</div>
    <div class=""rev-cols"" id=""rev-header""></div>
    <div class=""stats-col"">Pass Rate</div>
  </div>
  <div id=""rows""></div>
</div>
<div id=""no-match"">No assets match your search.</div>

<script>
const DATA={jsonSb};
const REVS=[{revArray}];
const S=['p','f','s'];

// Build revision header
const rh=document.getElementById('rev-header');
REVS.forEach(r=>{{const d=document.createElement('div');d.className='rev-col';d.textContent=r;rh.appendChild(d)}});

// Build rows
const rowsEl=document.getElementById('rows');
DATA.forEach((a,idx)=>{{
  const row=document.createElement('div');
  row.className='asset-row';
  row.dataset.idx=idx;
  row.dataset.name=a.name.toLowerCase();
  row.dataset.p=a.p;row.dataset.f=a.f;row.dataset.s=a.s;
  const total=a.p+a.f+a.s;
  const pct=total>0?((a.p/total)*100):0;
  row.dataset.pct=pct;

  let cells='';
  REVS.forEach(r=>{{
    const rv=a.revs[r];
    const cls=rv?S[rv[0]]:'n';
    cells+=`<div class=""cell ${{cls}}"" data-rev=""${{r}}""></div>`;
  }});

  const pctClass=pct===100?'perfect':pct>=80?'good':pct>=50?'mid':'bad';
  let badges='';
  if(a.f>0)badges+=`<span class=""count-badge f-badge"">${{a.f}}</span>`;
  if(a.s>0)badges+=`<span class=""count-badge s-badge"">${{a.s}}</span>`;

  row.innerHTML=`
    <div class=""name-col""><span class=""expand-icon"">&#9654;</span>${{a.name}}</div>
    <div class=""rev-cols"">${{cells}}</div>
    <div class=""stats-col"">${{badges}}<span class=""pass-pct ${{pctClass}}"">${{pct.toFixed(0)}}%</span></div>`;
  row.onclick=e=>{{if(e.target.classList.contains('cell'))return;toggleDetail(idx)}};

  const detail=document.createElement('div');
  detail.className='detail-panel';
  detail.id='detail-'+idx;

  rowsEl.appendChild(row);
  rowsEl.appendChild(detail);
}});

// Cell click -> show detail and scroll to that revision
document.querySelectorAll('.cell').forEach(c=>{{
  c.onclick=e=>{{
    e.stopPropagation();
    const row=c.closest('.asset-row');
    const idx=parseInt(row.dataset.idx);
    const rev=c.dataset.rev;
    openDetail(idx,rev);
  }};
}});

function toggleDetail(idx){{
  const panel=document.getElementById('detail-'+idx);
  const row=panel.previousElementSibling;
  const icon=row.querySelector('.expand-icon');
  if(panel.classList.contains('open')){{
    panel.classList.remove('open');
    icon.classList.remove('open');
  }}else{{
    openDetail(idx);
  }}
}}

function openDetail(idx,highlightRev){{
  const a=DATA[idx];
  const panel=document.getElementById('detail-'+idx);
  const row=panel.previousElementSibling;
  const icon=row.querySelector('.expand-icon');

  // Close all other panels
  document.querySelectorAll('.detail-panel.open').forEach(p=>{{
    if(p!==panel){{p.classList.remove('open');p.previousElementSibling.querySelector('.expand-icon').classList.remove('open')}}
  }});

  let rows='';
  const sortedRevs=Object.keys(a.revs).map(Number).sort((x,y)=>x-y);
  sortedRevs.forEach(r=>{{
    const rv=a.revs[r];
    const status=['Passed','Failed','Skipped'][rv[0]];
    const cls=['green','red','orange'][rv[0]];
    const err=rv[1]||'';
    const hl=r==highlightRev?'background:#1e3a5f':'';
    if(rv[0]===0&&!highlightRev)return; // skip passed rows unless clicking specific cell
    rows+=`<tr style=""${{hl}}""><td style=""width:60px;color:#${{cls}}"">Rev ${{r}}</td><td style=""width:80px;color:#${{cls}}"">&#${{rv[0]===0?'10003':rv[0]===1?'10007':'8856'}}; ${{status}}</td><td class=""err"">${{escapeHtml(err)}}</td></tr>`;
  }});

  if(!rows)rows='<tr><td colspan=""3"" style=""color:#555;text-align:center;padding:16px"">All revisions passed.</td></tr>';

  panel.innerHTML=`<table><thead><tr><th>Rev</th><th>Status</th><th>Details</th></tr></thead><tbody>${{rows}}</tbody></table>`;
  panel.classList.add('open');
  icon.classList.add('open');
}}

function escapeHtml(t){{return t.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;')}}

let currentFilter='all';
function setFilter(btn){{
  document.querySelectorAll('.btn-group .btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  currentFilter=btn.dataset.filter;
  applyFilters();
}}

function applyFilters(){{
  const q=document.getElementById('search').value.toLowerCase();
  let visible=0;
  document.querySelectorAll('.asset-row').forEach(row=>{{
    const name=row.dataset.name;
    const f=parseInt(row.dataset.f);
    const s=parseInt(row.dataset.s);
    const pct=parseFloat(row.dataset.pct);
    let show=true;
    if(q&&!name.includes(q))show=false;
    if(currentFilter==='failures'&&f===0)show=false;
    if(currentFilter==='perfect'&&pct!==100)show=false;
    if(currentFilter==='skipped'&&s===0)show=false;
    row.classList.toggle('hidden',!show);
    const detail=row.nextElementSibling;
    if(!show&&detail)detail.classList.remove('open');
    if(show)visible++;
  }});
  document.getElementById('no-match').style.display=visible===0?'block':'none';
}}

function applySort(){{
  const sortBy=document.getElementById('sort').value;
  const container=document.getElementById('rows');
  const pairs=[];
  const children=container.children;
  for(let i=0;i<children.length;i+=2){{
    pairs.push([children[i],children[i+1]]);
  }}
  pairs.sort((a,b)=>{{
    const ra=a[0],rb=b[0];
    if(sortBy==='name')return ra.dataset.name.localeCompare(rb.dataset.name);
    if(sortBy==='failures')return parseInt(rb.dataset.f)-parseInt(ra.dataset.f);
    if(sortBy==='passrate')return parseFloat(ra.dataset.pct)-parseFloat(rb.dataset.pct);
    if(sortBy==='skipped')return parseInt(rb.dataset.s)-parseInt(ra.dataset.s);
    return 0;
  }});
  pairs.forEach(p=>{{container.appendChild(p[0]);container.appendChild(p[1])}});
}}
</script>
</body>
</html>");

        string reportContent = html.ToString();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        File.WriteAllText(outputPath, reportContent);
        return outputPath;
    }

    private static string JsonEscape(string text)
    {
        return "\"" + text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t") + "\"";
    }
}
